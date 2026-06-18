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
          DEFAULT: '#3b82f6', // blue-500
          foreground: '#ffffff',
        },
        success: {
          DEFAULT: '#22c55e', // green-500
          foreground: '#ffffff',
        },
        warning: {
          DEFAULT: '#f59e0b', // amber-500
          foreground: '#ffffff',
        },
        destructive: {
          DEFAULT: '#ef4444', // red-500
          foreground: '#ffffff',
        },
        background: '#f8fafc', // slate-50
        card: '#ffffff',
        border: '#e2e8f0', // slate-200
        text: '#0f172a', // slate-900
        muted: '#64748b', // slate-500
      }
    },
  },
  plugins: [],
}
