using backend.DTOs.Order;
using backend.Models;
using backend.Repositories;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Constants;

namespace backend.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<CheckoutResponse> CreateOrderAsync(CheckoutRequest request)
        {
            if (request.Items == null || !request.Items.Any())
                throw new InvalidOperationException(AppConstants.Errors.EmptyCart);

            // Get authenticated user ID
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(AppConstants.Claims.UserId);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int authenticatedUserId))
            {
                _logger.LogError(AppConstants.Errors.Unauthorized);
                throw new UnauthorizedAccessException(AppConstants.Errors.Unauthorized);
            }

            // Optional: validate request.UserId
            if (request.UserId != 0 && request.UserId != authenticatedUserId)
            {
                _logger.LogWarning(
                    "Checkout request UserId ({RequestUserId}) does not match authenticated user UserId ({AuthenticatedUserId}). Using authenticated user ID.",
                    request.UserId, authenticatedUserId
                );
            }

            if (!await _userRepository.UserExistsByIdAsync(authenticatedUserId))
            {
                _logger.LogError(AppConstants.Errors.UserNotFound);
                throw new KeyNotFoundException(AppConstants.Errors.UserNotFound);
            }

            decimal totalPrice = request.Items.Sum(i => i.Price * i.Quantity);

            var order = new Order
            {
                UserId = authenticatedUserId,
                DeliveryAddress = request.DeliveryAddress,
                TotalPrice = totalPrice,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                OrderItems = request.Items.Select(ci => new OrderItem
                {
                    ItemId = ci.ItemId,
                    Quantity = ci.Quantity,
                    PriceAtPurchase = ci.Price
                }).ToList()
            };

            var createdOrder = await _orderRepository.CreateOrderAsync(order);
            _logger.LogInformation(AppConstants.Logs.CheckoutSuccess, createdOrder.OrderId, createdOrder.UserId);

            return new CheckoutResponse
            {
                Message = AppConstants.Messages.OrderPlaced,
                OrderId = createdOrder.OrderId
            };
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
        {
            int authenticatedUserId = GetAuthenticatedUserId();

            bool isAdmin = _httpContextAccessor.HttpContext?.User?.IsInRole(AppConstants.Roles.Admin) ?? false;

            if (authenticatedUserId != userId && !isAdmin)
            {
                _logger.LogWarning("User {AuthenticatedUserId} attempted to access orders for different user {RequestedUserId}.",
                    authenticatedUserId, userId);
                throw new UnauthorizedAccessException(AppConstants.Errors.Unauthorized);
            }

            if (!await _userRepository.UserExistsByIdAsync(userId))
            {
                _logger.LogWarning(AppConstants.Logs.FetchUserNotFound, userId);
                throw new KeyNotFoundException(AppConstants.Errors.UserNotFound);
            }

            _logger.LogInformation(AppConstants.Logs.FetchOrdersByUser, userId);
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            _logger.LogInformation(AppConstants.Logs.FetchOrdersCount, orders.Count(), userId);

            return orders.Select(MapOrderToDto);
        }

        public async Task<OrderDto> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdWithItemsAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning(AppConstants.Errors.OrderNotFound, orderId);
                throw new KeyNotFoundException(string.Format(AppConstants.Errors.OrderNotFound, orderId));
            }

            int authenticatedUserId = GetAuthenticatedUserId();
            bool isAdmin = _httpContextAccessor.HttpContext?.User?.IsInRole(AppConstants.Roles.Admin) ?? false;

            if (authenticatedUserId != order.UserId && !isAdmin)
            {
                _logger.LogWarning("User {AuthenticatedUserId} attempted to access order {OrderId} belonging to user {OrderUserId}.",
                    authenticatedUserId, orderId, order.UserId);
                throw new UnauthorizedAccessException(AppConstants.Errors.Unauthorized);
            }

            return MapOrderToDto(order);
        }

        public async Task CancelOrderAsync(int orderId)
        {
            _logger.LogInformation(AppConstants.Logs.OrderCancelAttempt, orderId);

            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning(AppConstants.Errors.OrderNotFound, orderId);
                throw new KeyNotFoundException(string.Format(AppConstants.Errors.OrderNotFound, orderId));
            }

            int authenticatedUserId = GetAuthenticatedUserId();
            bool isAdmin = _httpContextAccessor.HttpContext?.User?.IsInRole(AppConstants.Roles.Admin) ?? false;

            if (authenticatedUserId != order.UserId && !isAdmin)
            {
                _logger.LogWarning("User {AuthenticatedUserId} attempted to cancel order {OrderId} belonging to user {OrderUserId}.",
                    authenticatedUserId, orderId, order.UserId);
                throw new UnauthorizedAccessException(AppConstants.Errors.Unauthorized);
            }

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException(string.Format(AppConstants.Errors.CannotCancelOrder, order.Status));

            order.Status = OrderStatus.Cancelled;
            await _orderRepository.SaveChangesAsync();

            _logger.LogInformation(AppConstants.Messages.OrderCancelled);
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            bool isAdmin = _httpContextAccessor.HttpContext?.User?.IsInRole(AppConstants.Roles.Admin) ?? false;
            if (!isAdmin)
            {
                _logger.LogWarning("Non-admin user attempted to access GetAllOrdersAsync.");
                throw new UnauthorizedAccessException(AppConstants.Errors.Unauthorized);
            }

            _logger.LogInformation(AppConstants.Logs.AdminFetchAllOrders);
            var orders = await _orderRepository.GetAllOrdersAsync();
            return orders.Select(MapOrderToDto);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request)
        {
            bool isAdmin = _httpContextAccessor.HttpContext?.User?.IsInRole(AppConstants.Roles.Admin) ?? false;
            if (!isAdmin)
            {
                _logger.LogWarning("Non-admin user attempted to update order status for {OrderId}.", orderId);
                throw new UnauthorizedAccessException(AppConstants.Errors.Unauthorized);
            }

            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning(AppConstants.Errors.OrderNotFound, orderId);
                throw new KeyNotFoundException(string.Format(AppConstants.Errors.OrderNotFound, orderId));
            }

            order.Status = request.Status;
            await _orderRepository.SaveChangesAsync();

            _logger.LogInformation(AppConstants.Logs.AdminUpdateOrderStatus, orderId, request.Status);

            var updatedOrder = await _orderRepository.GetOrderByIdWithItemsAsync(orderId);
            return MapOrderToDto(updatedOrder!);
        }

        // Helper to get authenticated user ID safely
        private int GetAuthenticatedUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(AppConstants.Claims.UserId);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogError(AppConstants.Errors.Unauthorized);
                throw new UnauthorizedAccessException(AppConstants.Errors.Unauthorized);
            }

            return userId;
        }

        // Helper mapping method
        private OrderDto MapOrderToDto(Order order)
        {
            return new OrderDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                UserName = order.User?.UserName ?? AppConstants.Placeholders.NotAvailable,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                OrderDate = order.OrderDate,
                DeliveryAddress = order.DeliveryAddress,
                OrderItems = order.OrderItems.Select(oi =>
                {
                    if (oi.MenuItem == null)
                        _logger.LogWarning(AppConstants.Logs.FetchFail, oi.ItemId);

                    return new OrderItemDto
                    {
                        OrderItemId = oi.OrderItemId,
                        Quantity = oi.Quantity,
                        PriceAtPurchase = oi.PriceAtPurchase,
                        ItemName = oi.MenuItem?.Name ?? string.Format(AppConstants.Placeholders.UnknownItem, oi.ItemId),
                        ItemId = oi.MenuItem?.ItemId ?? oi.ItemId,
                        Description = oi.MenuItem?.Description,
                        ImageUrl = oi.MenuItem?.ImageUrl
                    };
                }).ToList()
            };
        }
    }
}
