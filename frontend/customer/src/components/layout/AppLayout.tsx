import { Outlet, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Printer, Menu, X, User } from 'lucide-react';
import { useState } from 'react';

const LanguageToggle = () => {
  const { i18n } = useTranslation();
  return (
    <button
      onClick={() => i18n.changeLanguage(i18n.language === 'ar' ? 'en' : 'ar')}
      className="px-3 py-1 rounded border border-gray-300 text-sm font-medium hover:bg-gray-100 transition"
    >
      {i18n.language === 'ar' ? 'English' : 'عربي'}
    </button>
  );
};

export const AppLayout = () => {
  const { t } = useTranslation();
  const [menuOpen, setMenuOpen] = useState(false);

  return (
    <div className="min-h-screen flex flex-col bg-gray-50">
      <header className="bg-white shadow-sm sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16 items-center">
            <div className="flex items-center gap-2">
              <Link to="/" className="flex items-center gap-2 text-brand font-bold text-xl">
                <Printer className="h-6 w-6" />
                <span>PrintPlatform</span>
              </Link>
            </div>

            {/* Desktop Nav */}
            <nav className="hidden md:flex gap-6 items-center">
              <Link to="/quotes" className="text-gray-600 hover:text-brand font-medium">{t('nav.quotes')}</Link>
              <Link to="/orders" className="text-gray-600 hover:text-brand font-medium">{t('nav.orders')}</Link>
              <Link to="/loyalty" className="text-gray-600 hover:text-brand font-medium">{t('nav.loyalty')}</Link>
              <Link to="/quotes/new" className="bg-brand text-white px-4 py-2 rounded-md font-medium hover:bg-blue-600 transition">
                {t('nav.getQuote')}
              </Link>
              <LanguageToggle />
              <Link to="/login" className="text-gray-600 hover:text-brand"><User className="h-5 w-5" /></Link>
            </nav>

            {/* Mobile menu button */}
            <div className="flex md:hidden items-center gap-4">
              <LanguageToggle />
              <button onClick={() => setMenuOpen(!menuOpen)} className="text-gray-600">
                {menuOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
              </button>
            </div>
          </div>
        </div>

        {/* Mobile Nav */}
        {menuOpen && (
          <div className="md:hidden border-t">
            <div className="px-2 pt-2 pb-3 space-y-1 sm:px-3 flex flex-col">
              <Link to="/quotes" className="block px-3 py-2 rounded-md text-base font-medium text-gray-700 hover:text-brand hover:bg-gray-50">{t('nav.quotes')}</Link>
              <Link to="/orders" className="block px-3 py-2 rounded-md text-base font-medium text-gray-700 hover:text-brand hover:bg-gray-50">{t('nav.orders')}</Link>
              <Link to="/loyalty" className="block px-3 py-2 rounded-md text-base font-medium text-gray-700 hover:text-brand hover:bg-gray-50">{t('nav.loyalty')}</Link>
              <Link to="/quotes/new" className="block px-3 py-2 rounded-md text-base font-medium text-brand hover:bg-gray-50">{t('nav.getQuote')}</Link>
              <Link to="/login" className="block px-3 py-2 rounded-md text-base font-medium text-gray-700 hover:text-brand hover:bg-gray-50">{t('nav.login')}</Link>
            </div>
          </div>
        )}
      </header>

      <main className="flex-1 w-full max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <Outlet />
      </main>

      <footer className="bg-white border-t py-8">
        <div className="max-w-7xl mx-auto px-4 text-center text-gray-500 text-sm">
          &copy; {new Date().getFullYear()} PrintPlatform EGY. All rights reserved.
        </div>
      </footer>
    </div>
  );
};
