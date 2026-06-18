import { useTranslation } from 'react-i18next';
import { useForm } from 'react-form'; // Wait, I need react-hook-form
import { useForm as useHookForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link } from 'react-router-dom';

const schema = z.object({
  identifier: z.string().min(1, { message: 'Required' }),
  password: z.string().min(6, { message: 'Required' }),
});

type LoginForm = z.infer<typeof schema>;

export default function Login() {
  const { t } = useTranslation();
  const { register, handleSubmit, formState: { errors } } = useHookForm<LoginForm>({
    resolver: zodResolver(schema)
  });

  const onSubmit = (data: LoginForm) => {
    console.log('Login data', data);
    // TODO: implement login mutation
  };

  return (
    <div className="max-w-md mx-auto mt-10 p-6 bg-white rounded-xl shadow-sm border border-gray-100">
      <h2 className="text-2xl font-bold mb-6 text-center">{t('auth.loginTitle')}</h2>
      
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-1">{t('auth.emailOrPhone')}</label>
          <input 
            {...register('identifier')} 
            className="w-full border rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-brand" 
            dir="auto"
          />
          {errors.identifier && <p className="text-red-500 text-xs mt-1">{errors.identifier.message}</p>}
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">{t('auth.password')}</label>
          <input 
            type="password"
            {...register('password')} 
            className="w-full border rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-brand" 
          />
          {errors.password && <p className="text-red-500 text-xs mt-1">{errors.password.message}</p>}
        </div>

        <div className="flex items-center">
          <input type="checkbox" id="remember" className="rounded text-brand focus:ring-brand ml-2 mr-2" />
          <label htmlFor="remember" className="text-sm text-gray-600">{t('auth.rememberMe')}</label>
        </div>

        <button type="submit" className="w-full bg-brand text-white py-2 rounded-md font-bold hover:bg-blue-600 transition">
          {t('auth.submitLogin')}
        </button>
      </form>
      
      <p className="mt-4 text-center text-sm text-gray-600">
        <Link to="/register" className="text-brand hover:underline">{t('nav.register')}</Link>
      </p>
    </div>
  );
}
