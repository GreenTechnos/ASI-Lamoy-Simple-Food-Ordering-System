import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import AdminNavigation from '../../../components/admin/adminNavbar';
import bgImage from '../../../assets/MAIN4.png';
import bowlImage from '../../../assets/BOWL.png';
import { useAuth } from '../../../context/AuthContext';

const AdminCreateMenu = () => {
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();
  useEffect(() => {
    if (!isAuthenticated) navigate('/login');
    else if (user?.role !== 'Admin') navigate('/');
  }, [isAuthenticated, user, navigate]);


  const [formData, setFormData] = useState({
    name: '',
    description: '',
    price: '',
    category: '',
    image: null,
    isActive: true
  });

  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [imagePreview, setImagePreview] = useState(null);

  // Animation state
  const [isVisible, setIsVisible] = useState({
    hero: false,
    content: false,
    form: false
  });

  const categories = ['Main Course', 'Dessert', 'Beverages'];

  // Intersection Observer for scroll animations
  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            setIsVisible(prev => ({
              ...prev,
              [entry.target.dataset.section]: true
            }));
          }
        });
      },
      {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
      }
    );

    const sections = document.querySelectorAll('[data-section]');
    sections.forEach(section => observer.observe(section));

    return () => observer.disconnect();
  }, []);

  // Set hero section visible immediately on mount
  useEffect(() => {
    const timer = setTimeout(() => {
      setIsVisible(prev => ({ ...prev, hero: true }));
    }, 100);
    return () => clearTimeout(timer);
  }, []);

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));

    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  const handleImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      // Validate file type
      const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
      if (!validTypes.includes(file.type)) {
        setErrors(prev => ({
          ...prev,
          image: 'Please select a valid image file (JPEG, PNG, GIF)'
        }));
        return;
      }

      // Validate file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        setErrors(prev => ({
          ...prev,
          image: 'Image size must be less than 5MB'
        }));
        return;
      }

      setFormData(prev => ({
        ...prev,
        image: file
      }));

      // Create preview
      const reader = new FileReader();
      reader.onloadend = () => {
        setImagePreview(reader.result);
      };
      reader.readAsDataURL(file);

      // Clear image error
      if (errors.image) {
        setErrors(prev => ({
          ...prev,
          image: ''
        }));
      }
    }
  };

  const validateForm = () => {
    const newErrors = {};

    if (!formData.name.trim()) {
      newErrors.name = 'Item name is required';
    } else if (formData.name.length < 3) {
      newErrors.name = 'Item name must be at least 3 characters';
    }

    if (!formData.description.trim()) {
      newErrors.description = 'Description is required';
    } else if (formData.description.length < 10) {
      newErrors.description = 'Description must be at least 10 characters';
    }

    if (!formData.price) {
      newErrors.price = 'Price is required';
    } else if (isNaN(formData.price) || parseFloat(formData.price) <= 0) {
      newErrors.price = 'Price must be a valid number greater than 0';
    }

    if (!formData.category) {
      newErrors.category = 'Category is required';
    }

    return newErrors;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const newErrors = validateForm();
    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    setIsSubmitting(true);

    try {
      // 🆕 Actual API call
      const formDataToSend = new FormData();
      formDataToSend.append('Name', formData.name);
      formDataToSend.append('Description', formData.description);
      formDataToSend.append('Price', formData.price);
      formDataToSend.append('Category', formData.category);
      formDataToSend.append('IsActive', formData.isActive);
      if (formData.image) formDataToSend.append('Image', formData.image);

      const response = await fetch('http://localhost:5143/api/admin/menu', {
        method: 'POST',
        body: formDataToSend,
        // If you’re using JWT:
        headers: {
          Authorization: `Bearer ${localStorage.getItem('token') || ''}` // 🆕
        }
      });

      if (!response.ok) {
        const errMsg = await response.text();
        throw new Error(errMsg || 'Failed to create menu item.');
      }

      const createdItem = await response.json();
      console.log('Created menu item:', createdItem);

      navigate('/admin/menu', {
        state: {
          message: `Menu item "${formData.name}" created successfully!`,
          type: 'success'
        }
      });

    } catch (error) {
      console.error('Error creating menu item:', error);
      setErrors({ submit: error.message || 'Failed to create menu item. Please try again.' });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen relative font-sans bg-gray-50">
      <div className="absolute w-full z-50">
        <AdminNavigation />
      </div>

      {/* Hero Section */}
      <div
        className="relative top-0 left-0 w-full h-96 bg-cover bg-center bg-no-repeat flex flex-col items-center justify-center px-4 sm:px-6 lg:px-8 py-12 sm:py-16 text-center"
        style={{
          backgroundImage: `url(${bgImage})`,
          backgroundSize: 'cover',
          backgroundPosition: 'center center'
        }}
      >
        <div
          className={`transition-all duration-1000 ease-out ${isVisible.hero ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
            }`}
          data-section="hero"
        >
          <h1 className="text-white text-4xl sm:text-5xl md:text-6xl font-bold mb-6">
            Create Menu Item
          </h1>
          <p className="text-white text-base sm:text-lg md:text-xl max-w-2xl leading-relaxed">
            Add a new delicious item to your restaurant menu
          </p>
        </div>

        {/* Back to Menu button */}
        <div className="absolute left-1/2 bottom-0 transform -translate-x-1/2 translate-y-1/2 z-30">
          <div className="bg-white/20 backdrop-blur-sm rounded-full p-3">
            <button
              onClick={() => navigate('/admin/menu')}
              className="bg-white px-8 py-3 rounded-full font-semibold text-lg text-yellow-500 hover:bg-gray-50 transition-colors shadow-lg"
            >
              Back to Menu
            </button>
          </div>
        </div>
      </div>

      {/* Content Section */}
      <div className="bg-gray-50 pt-20 pb-8">
        <div className="max-w-4xl mx-auto px-4">
          {/* Breadcrumb */}
          <div
            className={`flex items-center justify-between mb-8 transition-all duration-800 ease-out ${isVisible.content ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
              }`}
            data-section="content"
          >
            <div className="flex items-center space-x-2 text-sm text-gray-900">
              <button onClick={() => navigate('/admin/menu')} className="hover:text-yellow-600 font-medium">Menu</button>
              <span>/</span>
              <span className="text-gray-900 font-semibold">Create Item</span>
            </div>
          </div>

          {/* Form Section */}
          <div
            className={`bg-white rounded-xl border border-gray-200 p-8 transition-all duration-1000 ease-out ${isVisible.form ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-12'
              }`}
            data-section="form"
          >
            <h2 className="text-2xl font-bold text-gray-900 mb-6">Menu Item Details</h2>

            <form onSubmit={handleSubmit} className="space-y-8">
              {/* Item Name */}
              <div>
                <label htmlFor="name" className="block text-sm font-semibold text-gray-800 mb-3">
                  Item Name *
                </label>
                <input
                  type="text"
                  id="name"
                  name="name"
                  value={formData.name}
                  onChange={handleInputChange}
                  className={`block w-full px-4 py-4 border-1 rounded-xl focus:ring-2 focus:ring-yellow-400 focus:border-yellow-400 focus:outline-none transition-all duration-300 text-gray-800 placeholder-gray-400 bg-white ${errors.name ? 'border-red-400 focus:border-red-400 focus:ring-red-400' : 'border-gray-200 hover:border-gray-300'
                    }`}
                  placeholder="Enter item name (e.g., Chicken Adobo)"
                />
                {errors.name && <p className="mt-2 text-sm text-red-600 font-medium">{errors.name}</p>}
              </div>

              {/* Description */}
              <div>
                <label htmlFor="description" className="block text-sm font-semibold text-gray-800 mb-3">
                  Description *
                </label>
                <textarea
                  id="description"
                  name="description"
                  rows={5}
                  value={formData.description}
                  onChange={handleInputChange}
                  maxLength={500}
                  className={`block w-full px-4 py-4 border-2 rounded-xl focus:ring-2 focus:ring-yellow-400 focus:border-yellow-400 focus:outline-none transition-all duration-300 resize-none text-gray-800 placeholder-gray-400 bg-white ${errors.description ? 'border-red-400 focus:border-red-400 focus:ring-red-400' : 'border-gray-200 hover:border-gray-300'
                    }`}
                  placeholder="Describe the dish, ingredients, and what makes it special..."
                />
                <div className="mt-2 flex justify-between items-center">
                  {errors.description ?
                    <p className="text-sm text-red-600 font-medium">{errors.description}</p> :
                    <p className="text-sm text-gray-600">Provide a detailed description of the menu item</p>
                  }
                  <span className={`text-sm font-medium ${formData.description.length > 450 ? 'text-red-500' : 'text-gray-500'}`}>
                    {formData.description.length}/500
                  </span>
                </div>
              </div>

              {/* Price and Category Row */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                {/* Price */}
                <div>
                  <label htmlFor="price" className="block text-sm font-semibold text-gray-800 mb-3">
                    Price (₱) *
                  </label>
                  <div className="relative">
                    <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                      <span className="text-gray-500 text-lg font-medium">₱</span>
                    </div>
                    <input
                      type="number"
                      id="price"
                      name="price"
                      step="0.01"
                      min="0"
                      value={formData.price}
                      onChange={handleInputChange}
                      className={`block w-full pl-10 pr-4 py-4 border-2 rounded-xl focus:ring-2 focus:ring-yellow-400 focus:border-yellow-400 focus:outline-none transition-all duration-300 text-gray-800 placeholder-gray-400 bg-white ${errors.price ? 'border-red-400 focus:border-red-400 focus:ring-red-400' : 'border-gray-200 hover:border-gray-300'
                        }`}
                      placeholder="0.00"
                    />
                  </div>
                  {errors.price && <p className="mt-2 text-sm text-red-600 font-medium">{errors.price}</p>}
                </div>

                {/* Category */}
                <div>
                  <label htmlFor="category" className="block text-sm font-semibold text-gray-800 mb-3">
                    Category *
                  </label>
                  <div className="relative">
                    <select
                      id="category"
                      name="category"
                      value={formData.category}
                      onChange={handleInputChange}
                      className={`block w-full appearance-none px-4 py-4 pr-12 border-2 rounded-xl focus:ring-2 focus:ring-yellow-400 focus:border-yellow-400 focus:outline-none transition-all duration-300 text-gray-800 bg-white cursor-pointer ${errors.category ? 'border-red-400 focus:border-red-400 focus:ring-red-400' : 'border-gray-200 hover:border-gray-300'
                        }`}
                    >
                      <option value="" className="text-gray-400">Select a category</option>
                      {categories.map(category => (
                        <option key={category} value={category} className="text-gray-800">{category}</option>
                      ))}
                    </select>
                    <div className="absolute inset-y-0 right-0 flex items-center pr-4 pointer-events-none">
                      <svg className="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                      </svg>
                    </div>
                  </div>
                  {errors.category && <p className="mt-2 text-sm text-red-600 font-medium">{errors.category}</p>}
                </div>
              </div>

              {/* Image Upload */}
              <div>
                <label htmlFor="image" className="block text-sm font-semibold text-gray-800 mb-3">
                  Item Image
                </label>
                <div className="space-y-4">
                  <div className={`flex justify-center px-6 pt-8 pb-8 border-2 border-dashed rounded-xl transition-all duration-300 ${errors.image ? 'border-red-400' : 'border-gray-300 hover:border-yellow-400'
                    }`}>
                    <div className="space-y-2 text-center">
                      {imagePreview ? (
                        <div className="relative inline-block">
                          <img
                            src={imagePreview}
                            alt="Preview"
                            className="mx-auto h-40 w-40 object-cover rounded-xl shadow-lg"
                          />
                          <button
                            type="button"
                            onClick={() => {
                              setImagePreview(null);
                              setFormData(prev => ({ ...prev, image: null }));
                            }}
                            className="absolute -top-2 -right-2 bg-red-500 hover:bg-red-600 text-white rounded-full w-8 h-8 flex items-center justify-center text-sm font-bold transition-colors duration-200 shadow-lg"
                          >
                            ×
                          </button>
                        </div>
                      ) : (
                        <>
                          <svg className="mx-auto h-16 w-16 text-gray-400" stroke="currentColor" fill="none" viewBox="0 0 48 48">
                            <path d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8m-12 4h.02" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" />
                          </svg>
                          <div className="text-gray-600">
                            <label htmlFor="image" className="relative cursor-pointer bg-white rounded-lg font-semibold text-yellow-500 hover:text-yellow-600 focus-within:outline-none focus-within:ring-2 focus-within:ring-yellow-400 px-2 py-1 transition-colors duration-200">
                              <span>Upload an image</span>
                              <input
                                id="image"
                                name="image"
                                type="file"
                                accept="image/*"
                                onChange={handleImageChange}
                                className="sr-only focus:outline-none"
                              />
                            </label>
                            <span className="text-gray-500"> or drag and drop</span>
                          </div>
                          <p className="text-sm text-gray-500 font-medium">PNG, JPG, GIF up to 5MB</p>
                        </>
                      )}
                    </div>
                  </div>
                  {errors.image && <p className="text-sm text-red-600 font-medium">{errors.image}</p>}
                </div>
              </div>

              {/* Status */}
              <div className="bg-gray-50 rounded-xl p-6">
                <label className="flex items-start gap-4 cursor-pointer">
                  <input
                    type="checkbox"
                    name="isActive"
                    checked={formData.isActive}
                    onChange={handleInputChange}
                    className="w-5 h-5 text-yellow-500 bg-gray-100 border-gray-300 rounded focus:ring-yellow-400 focus:ring-2 focus:outline-none mt-0.5 transition-colors duration-200"
                  />
                  <div>
                    <span className="text-base font-semibold text-gray-800 block">
                      Make this item active and available for ordering
                    </span>
                    <span className="text-sm text-gray-600 mt-1 block">
                      Active items will be visible to customers and available for purchase
                    </span>
                  </div>
                </label>
              </div>

              {/* Submit Error */}
              {errors.submit && (
                <div className="bg-red-50 border border-red-200 rounded-xl p-4">
                  <div className="flex items-center gap-3">
                    <svg className="w-5 h-5 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
                    </svg>
                    <p className="text-sm text-red-700 font-medium">{errors.submit}</p>
                  </div>
                </div>
              )}

              {/* Form Actions */}
              <div className="flex items-center justify-end space-x-4 pt-8 border-t border-gray-200">
                <button
                  type="button"
                  onClick={() => navigate('/admin/menu')}
                  className="bg-gray-500 hover:bg-gray-600 text-white px-8 py-3 rounded-xl font-semibold transition-all duration-200 hover:shadow-lg"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="bg-yellow-500 hover:bg-yellow-600 disabled:bg-yellow-300 disabled:cursor-not-allowed text-white px-10 py-3 rounded-xl font-semibold transition-all duration-200 hover:shadow-lg flex items-center gap-3"
                >
                  {isSubmitting ? (
                    <>
                      <svg className="animate-spin h-5 w-5" fill="none" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                        <path className="opacity-75" fill="currentColor" d="m4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                      </svg>
                      Creating...
                    </>
                  ) : (
                    <>
                      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                      </svg>
                      Create Item
                    </>
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminCreateMenu;