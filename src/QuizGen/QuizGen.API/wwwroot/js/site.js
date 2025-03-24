// Token management
function saveAuthToken(token, userId, userName) {
    // Save token and user info in cookies (secure would be better in production)
    const expiryDate = new Date();
    expiryDate.setTime(expiryDate.getTime() + (60 * 60 * 1000)); // 1 hour expiry
    
    document.cookie = `AuthToken=${token}; expires=${expiryDate.toUTCString()}; path=/`;
    document.cookie = `UserId=${userId}; expires=${expiryDate.toUTCString()}; path=/`;
    document.cookie = `UserName=${userName}; expires=${expiryDate.toUTCString()}; path=/`;
}

function getAuthToken() {
    return getCookie('AuthToken');
}

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
}

// API request helper with auth
async function apiRequest(url, method = 'GET', data = null) {
    const token = getAuthToken();
    const headers = {
        'Content-Type': 'application/json'
    };
    
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }
    
    const options = {
        method: method,
        headers: headers
    };
    
    if (data && (method === 'POST' || method === 'PUT')) {
        options.body = JSON.stringify(data);
    }
    
    try {
        const response = await fetch(url, options);
        const result = await response.json();
        
        if (!response.ok) {
            throw new Error(result.message || 'An error occurred');
        }
        
        return result;
    } catch (error) {
        console.error('API request error:', error);
        throw error;
    }
} 