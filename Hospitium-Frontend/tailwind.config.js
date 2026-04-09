/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './src/**/*.{html,ts}',
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#5A7A5A', // Sage
          hover: '#4B684B',
          active: '#3F593F',
          light: '#8AAE8A',
        },
        accent: {
          DEFAULT: '#C4A86A', // Gold
          soft: '#F4EDDC',   // For pending status bg
        },
        neutral: {
          bg: '#F4F6F4',
          surface: '#FFFFFF',
          sidebar: '#E7EFE7',
          border: '#D6E0D6',
          primary: '#2A2E2A',
          secondary: '#909890',
        },
        success: {
          bg: '#E0EDE0',
          text: '#3A5A3A',
        },
        warning: {
          bg: '#F4EDDC',
          text: '#7A6020',
        },
        error: {
          DEFAULT: '#A87070',
          bg: '#F0E4E4',
          text: '#7A3A3A',
        },
        booking: {
          active: {
            bg: '#EAF0EA',
            text: '#2A5A2A',
          },
          cancelled: {
            bg: '#EEEEEE',
            text: '#909890',
          },
          completed: '#5B6E8C'
        }
      },
      fontFamily: {
        'display': ['Outfit', 'sans-serif'],
        'body': ['Inter', 'sans-serif'],
      },
      animation: {
        'fade-in': 'fadeIn 0.3s ease-out',
      },
      keyframes: {
        fadeIn: {
          '0%':   { opacity: '0', transform: 'translateY(8px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
      },
    },
  },
  plugins: [],
};
