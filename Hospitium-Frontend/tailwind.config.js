const plugin = require('tailwindcss/plugin');

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './src/**/*.{html,ts,css}',
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
        'slide-up': 'slideUp 0.4s ease-out',
      },
      keyframes: {
        fadeIn: {
          '0%':   { opacity: '0', transform: 'translateY(8px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        slideUp: {
          '0%':   { opacity: '0', transform: 'translateY(20px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
      },
    },
  },
  plugins: [
    plugin(function({ addComponents }) {
      addComponents({
        '.btn-primary': {
          '@apply bg-primary hover:bg-primary-hover active:bg-primary-active text-white font-bold py-3 px-6 rounded-2xl transition-all shadow-lg shadow-primary/10 transform hover:-translate-y-0.5 active:scale-95 disabled:opacity-50 disabled:transform-none': {},
        },
        '.btn-secondary': {
          '@apply bg-neutral-bg hover:bg-neutral-border/50 text-neutral-primary font-bold py-3 px-6 rounded-2xl transition-all border border-neutral-border transform hover:-translate-y-0.5 active:scale-95': {},
        },
        '.btn-accent': {
          '@apply bg-accent hover:bg-accent/90 text-white font-bold py-3 px-6 rounded-2xl transition-all shadow-lg shadow-accent/10 transform hover:-translate-y-0.5 active:scale-95': {},
        },
        '.card-premium': {
          '@apply bg-neutral-surface border border-neutral-border rounded-[2rem] shadow-sm hover:shadow-md transition-all duration-300': {},
        },
        '.badge-success': {
          '@apply bg-success-bg text-success-text px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider border border-success-text/10': {},
        },
        '.badge-warning': {
          '@apply bg-warning-bg text-warning-text px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider border border-warning-text/10': {},
        },
        '.badge-error': {
          '@apply bg-error-bg text-error-text px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider border border-error-text/10': {},
        },
        '.badge-active': {
          '@apply bg-booking-active-bg text-booking-active-text px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider': {},
        },
        '.badge-cancelled': {
          '@apply bg-booking-cancelled-bg text-booking-cancelled-text px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider': {},
        },
        '.badge-completed': {
          'background-color': 'rgba(91, 110, 140, 0.1)',
          'color': '#5B6E8C',
          'border': '1px solid rgba(91, 110, 140, 0.3)',
          '@apply px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest': {},
        },
        '.input-premium': {
          '@apply w-full bg-neutral-bg border border-neutral-border rounded-xl px-4 py-3 text-neutral-primary focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all': {},
        },
        '.animate-fade-in': {
          'animation': 'fadeIn 0.3s ease-out',
        },
        /* ─── Auth Pages Shared Layout ───────────────── */
        '.auth-page': {
          '@apply min-h-screen flex items-center justify-center bg-neutral-bg px-4 py-12': {},
        },
        '.auth-card-wrap': {
          '@apply w-full max-w-md': {},
        },
        '.auth-card': {
          '@apply card-premium px-8 py-10': {},
        },
        /* ─── Header ─────────────────────────────────── */
        '.auth-header': {
          '@apply text-center mb-10': {},
        },
        '.auth-header-sm': {
          '@apply text-center mb-8': {},
        },
        '.auth-icon-wrap': {
          '@apply inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-primary mb-6 shadow-lg shadow-primary/20': {},
        },
        '.auth-icon': {
          '@apply w-8 h-8 text-white': {},
        },
        '.auth-title': {
          '@apply text-3xl font-bold text-neutral-primary tracking-tight font-display': {},
        },
        '.auth-subtitle': {
          '@apply text-neutral-secondary mt-2 font-medium': {},
        },
        /* ─── Form ────────────────────────────────────── */
        '.auth-form': {
          '@apply space-y-6': {},
        },
        '.auth-form-sm': {
          '@apply space-y-5': {},
        },
        '.auth-field': {
          '@apply block': {},
        },
        '.auth-label': {
          '@apply block text-xs font-bold text-neutral-secondary uppercase tracking-widest mb-2': {},
        },
        '.auth-label-row': {
          '@apply flex items-center justify-between mb-2': {},
        },
        '.auth-forgot-link': {
          '@apply text-xs text-primary hover:text-primary-hover font-bold transition-colors': {},
        },
        '.auth-input-wrap': {
          '@apply relative': {},
        },
        '.auth-toggle-pwd': {
          '@apply absolute right-4 top-1/2 -translate-y-1/2 text-neutral-secondary hover:text-neutral-primary transition-colors': {},
        },
        '.auth-toggle-icon': {
          '@apply w-5 h-5 opacity-70': {},
        },
        '.auth-error-text': {
          '@apply mt-2 text-xs text-error-text font-bold flex items-center gap-1.5': {},
        },
        '.auth-error-icon': {
          '@apply w-3.5 h-3.5': {},
        },
        '.auth-error-text-simple': {
          '@apply mt-2 text-xs text-error-text font-bold': {},
        },
        /* ─── Password Hint ───────────────────────────── */
        '.auth-pwd-hint': {
          '@apply mt-4 p-4 bg-primary/5 rounded-xl border border-primary/10 text-[11px] text-primary/80 leading-relaxed font-bold': {},
        },
        '.auth-pwd-hint-title': {
          '@apply uppercase tracking-widest mb-2 opacity-100': {},
        },
        '.auth-pwd-hint-grid': {
          '@apply grid grid-cols-2 gap-2': {},
        },
        /* ─── Error Banner ───────────────────────────── */
        '.auth-error-banner': {
          '@apply bg-error-bg text-error-text rounded-xl px-4 py-4 text-sm flex items-start gap-3 border border-error-text/10 shadow-sm animate-fade-in': {},
        },
        '.auth-error-banner-icon': {
          '@apply w-5 h-5 flex-shrink-0': {},
        },
        '.auth-error-banner-simple': {
          '@apply bg-error-bg text-error-text rounded-xl px-4 py-4 text-sm font-bold border border-error-text/10 shadow-sm animate-fade-in': {},
        },
        /* ─── Submit Button ───────────────────────────── */
        '.auth-spinner': {
          '@apply animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2': {},
        },
        '.auth-footer-text': {
          '@apply text-center text-sm text-neutral-secondary font-medium': {},
        },
        '.auth-footer-link': {
          '@apply text-primary hover:text-primary-hover font-bold transition-colors': {},
        },
        /* ─── Booking Filters (Shared: Admin + Manager) ─── */
        '.gbk-filters': {
          '@apply flex flex-wrap items-center gap-4': {},
        },
        '.gbk-select-wrap': {
          '@apply relative min-w-[160px]': {},
        },
        '.gbk-select': {
          '@apply w-full bg-white border border-neutral-border rounded-xl px-4 py-2.5 text-sm font-semibold focus:outline-none focus:ring-2 focus:ring-primary/20 transition-all appearance-none': {},
        },
        '.gbk-select-chevron': {
          '@apply absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none opacity-50': {},
        },
        '.gbk-chevron-icon': {
          '@apply w-4 h-4': {},
        },
        '.gbk-date-input': {
          '@apply bg-white border border-neutral-border rounded-xl px-4 py-2.5 text-sm font-semibold focus:outline-none focus:ring-2 focus:ring-primary/20 transition-all': {},
        },
        '.gbk-reset-btn': {
          '@apply px-4 py-2.5 rounded-xl border border-neutral-border text-neutral-secondary text-sm hover:bg-neutral-bg transition-colors font-bold': {},
        }
      })
    })
  ],
};
