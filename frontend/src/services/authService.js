import { API_BASE_URL } from '../apiConfig'; // Adjust the path '..' as needed

/**
 * A helper function to handle API responses and errors.
 * This is where we'll read the error message from your middleware.
 */
const handleResponse = async (response) => {
    if (response.ok) {
        // For 200 OK responses (like login) or 201 Created
        // If the response body is empty (like a 204 No Content), return an empty object
        const text = await response.text();
        return text ? JSON.parse(text) : {};
    }

    // If we get a 400, 401, 409, 500, etc., it's an error.
    // We expect a JSON body like: { "error": "Username already exists." }
    const errorData = await response.json();

    // Throw an error with the *exact message* from the backend.
    // The component's catch block will grab this.
    throw new Error(errorData.error || 'An unknown server error occurred.');
};

/**
 * Registers a new user.
 * @param {object} userData - The user registration data.
 */
export const register = async (userData) => {
    const response = await fetch(`${API_BASE_URL}/auth/register`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(userData),
    });
    return handleResponse(response);
};

/**
 * Logs in a user.
 * @param {object} credentials - { email, password }
 */
export const login = async (credentials) => {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(credentials),
    });
    return handleResponse(response);
};

/**
 * Requests a password reset.
 */
export const requestPasswordReset = async (email) => {
    const response = await fetch(`${API_BASE_URL}/user/request-password-reset`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email }),
    });
    return handleResponse(response);
};

/**
 * Resets the password.
 */
export const resetPassword = async (token, newPassword) => {
    const response = await fetch(`${API_BASE_URL}/user/reset-password`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ token, newPassword }),
    });
    return handleResponse(response);
};
