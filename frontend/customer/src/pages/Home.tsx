import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { Cuboid, Printer } from 'lucide-react';

export default function Home() {
  const { t } = useTranslation();

  return (
    <div className="flex flex-col items-center justify-center min-h-[80vh] text-center space-y-8">
      <div className="bg-brand/10 p-6 rounded-full mb-4">
        <Cuboid className="w-20 h-20 text-brand" />
      </div>
      <h1 className="text-4xl md:text-6xl font-extrabold tracking-tight text-gray-900 max-w-3xl">
        {t('home.heroTitle')}
      </h1>
      <p className="text-xl text-gray-600 max-w-2xl">
        {t('home.heroSubtitle')}
      </p>
      
      <div className="flex flex-col sm:flex-row gap-4 w-full sm:w-auto mt-8">
        <Link 
          to="/quotes/new" 
          className="bg-brand text-white px-8 py-4 rounded-lg font-bold text-lg hover:bg-blue-600 transition shadow-lg w-full sm:w-auto"
        >
          {t('home.ctaQuote')}
        </Link>
        <a 
          href="#" 
          className="bg-white text-gray-800 border border-gray-300 px-8 py-4 rounded-lg font-bold text-lg hover:bg-gray-50 transition w-full sm:w-auto flex items-center justify-center gap-2"
        >
          <Printer className="w-5 h-5" />
          {t('home.ctaJoin')}
        </a>
      </div>
    </div>
  );
}
