import tailwindcssRtl from 'tailwindcss-rtl';

/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#3b82f6',
          foreground: '#ffffff',
        },
        success: {
          DEFAULT: '#22c55e',
          foreground: '#ffffff',
        },
        warning: {
          DEFAULT: '#f59e0b',
          foreground: '#ffffff',
        },
        destructive: {
          DEFAULT: '#ef4444',
          foreground: '#ffffff',
        },
        background: '#f8fafc',
        card: '#ffffff',
        border: '#e2e8f0',
        text: '#0f172a',
        muted: '#64748b',
      }
    },
  },
  plugins: [tailwindcssRtl],
}