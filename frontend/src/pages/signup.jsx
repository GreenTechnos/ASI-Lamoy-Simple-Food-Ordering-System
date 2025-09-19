import React, { useState } from 'react';
import Navigation from '../components/navbar';
import bgImage from '../assets/MAIN4.png';
import { useNavigate } from 'react-router-dom';


const SignUp = () => {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [fullname, setFullname] = useState('');
  const [address, setAddress] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();

  const handleSignUp = async (e) => {
    e.preventDefault();

    try {
      const response = await fetch('http://localhost:5143/api/user', { // Change port if needed
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          userName: username,
          email: email,
          password: password,
          fullName: fullname,
          phoneNumber: phoneNumber,
          address: address,
        }),
      });

      if (response.ok) {
        const data = await response.json();
        alert(data.message || 'Account created successfully!'); // Use the data properly
        navigate('/login');  // Redirect to login page
      } else {
        const error = await response.text();
        alert(error); // Show error message
      }
    } catch (error) {
      console.error('Error during sign up:', error);
      alert('An error occurred. Please try again later.');
    }
  };

  const handleSignIn = () => {
    navigate('/login');
  };

  return (
    <div className="min-h-screen relative overflow-hidden font-sans">
      {/* Upper half with background image */}
      <div 
        className="absolute top-0 left-0 w-full h-1/2 bg-cover bg-center bg-no-repeat"
        style={{ 
          backgroundImage: `url(${bgImage})`,
          backgroundSize: 'cover',
          backgroundPosition: 'center center'
        }}
      />
      {/* Lower half with white background */}
      <div className="absolute bottom-0 left-0 w-full h-1/2 bg-white" />
      
      <div className="absolute w-full z-50">
        <Navigation />
      </div>
      <div className="relative z-10 flex flex-col items-center justify-center min-h-screen px-4 sm:px-6 lg:px-8 py-8 sm:py-12">
        <div className="text-center mb-6 sm:mb-8 mt-16 sm:mt-20">
          <h1 className="text-white text-2xl sm:text-3xl md:text-4xl font-bold mb-3 sm:mb-4 px-4">Get Started</h1>
          <p className="text-white/90 text-base sm:text-lg px-4 max-w-md sm:max-w-none mx-auto">
            Sign up now and enjoy quick, simple, and hassle-free
            <span className="hidden sm:inline"><br /></span>
            <span className="sm:hidden"> </span>
            food ordering—Filipino style.
          </p>
        </div>
        
        <div className="w-full max-w-xs sm:max-w-lg md:max-w-2xl bg-white/20 backdrop-blur-sm rounded-2xl sm:rounded-3xl p-3 sm:p-4">
          <div className="w-full bg-white rounded-2xl sm:rounded-3xl p-6 sm:p-8 md:p-10 border border-gray-100">
            <h2 className="text-xl sm:text-2xl md:text-3xl font-bold text-gray-700 mb-6 sm:mb-8 text-center">
              Sign Up to <span className="text-yellow-500">Lamoy</span>
            </h2>
          
            <form onSubmit={handleSignUp} className="space-y-4 sm:space-y-5">
              {/* Username and Full Name - responsive columns */}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4">
                <div>
                  <label htmlFor="username" className="block text-xs sm:text-sm font-medium text-gray-600 mb-1 sm:mb-2">
                    Username
                  </label>
                  <input
                    type="text"
                    id="username"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    className="w-full px-3 sm:px-4 py-2.5 sm:py-3 border border-gray-200 rounded-lg sm:rounded-xl focus:ring-2 focus:ring-yellow-100 focus:border-yellow-400 outline-none transition-all duration-200 bg-gray-50 text-gray-700 text-sm sm:text-base"
                    required
                  />
                </div>
                <div>
                  <label htmlFor="fullname" className="block text-xs sm:text-sm font-medium text-gray-600 mb-1 sm:mb-2">
                    Full Name
                  </label>
                  <input
                    type="text"
                    id="fullname"
                    value={fullname}
                    onChange={(e) => setFullname(e.target.value)}
                    className="w-full px-3 sm:px-4 py-2.5 sm:py-3 border border-gray-200 rounded-lg sm:rounded-xl focus:ring-2 focus:ring-yellow-100 focus:border-yellow-400 outline-none transition-all duration-200 bg-gray-50 text-gray-700 text-sm sm:text-base"
                    required
                  />
                </div>
              </div>
              
              <div>
                <label htmlFor="email" className="block text-xs sm:text-sm font-medium text-gray-600 mb-1 sm:mb-2">
                  Email
                </label>
                <input
                  type="email"
                  id="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="w-full px-3 sm:px-4 py-2.5 sm:py-3 border border-gray-200 rounded-lg sm:rounded-xl focus:ring-2 focus:ring-yellow-100 focus:border-yellow-400 outline-none transition-all duration-200 bg-gray-50 text-gray-700 text-sm sm:text-base"
                  required
                />
              </div>

              {/* Phone Number and Password - responsive columns */}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4">
                <div>
                  <label htmlFor="phoneNumber" className="block text-xs sm:text-sm font-medium text-gray-600 mb-1 sm:mb-2">
                    Phone Number
                  </label>
                  <input
                    type="tel"
                    id="phoneNumber"
                    value={phoneNumber}
                    onChange={(e) => setPhoneNumber(e.target.value)}
                    className="w-full px-3 sm:px-4 py-2.5 sm:py-3 border border-gray-200 rounded-lg sm:rounded-xl focus:ring-2 focus:ring-yellow-100 focus:border-yellow-400 outline-none transition-all duration-200 bg-gray-50 text-gray-700 text-sm sm:text-base"
                    required
                  />
                </div>
                <div>
                  <label htmlFor="password" className="block text-xs sm:text-sm font-medium text-gray-600 mb-1 sm:mb-2">
                    Password
                  </label>
                  <input
                    type="password"
                    id="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    className="w-full px-3 sm:px-4 py-2.5 sm:py-3 border border-gray-200 rounded-lg sm:rounded-xl focus:ring-2 focus:ring-yellow-100 focus:border-yellow-400 outline-none transition-all duration-200 bg-gray-50 text-gray-700 text-sm sm:text-base"
                    required
                  />
                </div>
              </div>

              <div>
                <label htmlFor="address" className="block text-xs sm:text-sm font-medium text-gray-600 mb-1 sm:mb-2">
                  Address
                </label>
                <textarea
                  id="address"
                  value={address}
                  onChange={(e) => setAddress(e.target.value)}
                  rows="2"
                  className="w-full px-3 sm:px-4 py-2.5 sm:py-3 border border-gray-200 rounded-lg sm:rounded-xl focus:ring-2 focus:ring-yellow-100 focus:border-yellow-400 outline-none transition-all duration-200 bg-gray-50 text-gray-700 resize-none text-sm sm:text-base"
                  required
                />
              </div>
              
              <button
                type="submit"
                className="w-full bg-yellow-500 hover:bg-yellow-600 text-white py-3 sm:py-4 px-4 rounded-lg sm:rounded-xl font-bold text-base sm:text-lg transition-all duration-200 mt-4 sm:mt-6"
              >
                Sign Up
              </button>
              
              <div className="text-center text-xs sm:text-sm text-gray-600 pt-2">
                Already have an Account?{' '}
                <button 
                  type="button"
                  onClick={handleSignIn}
                  className="text-yellow-500 hover:text-yellow-600 font-semibold hover:underline transition-all"
                >
                  Sign In
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SignUp;