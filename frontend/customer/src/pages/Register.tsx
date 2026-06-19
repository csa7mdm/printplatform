import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useRegisterCustomer, useVerifyOtp } from '../api/hooks';
import { Lang } from '../api/types';

const schema = z.object({
  nameAr: z.string().min(2, { message: 'Required' }),
  nameEn: z.string().min(2, { message: 'Required' }),
  email: z.string().email(),
  phone: z.string().min(10),
  password: z.string().min(6),
});

type RegisterForm = z.infer<typeof schema>;

export default function Register() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const [step, setStep] = useState<1 | 2>(1);
  const [phone, setPhone] = useState('');
  const [userId, setUserId] = useState('');
  const [otpCode, setOtpCode] = useState('');
  
  const registerMutation = useRegisterCustomer();
  const verifyOtpMutation = useVerifyOtp();

  const { register, handleSubmit, formState: { errors } } = useForm<RegisterForm>({
    resolver: zodResolver(schema)
  });

  const onSubmit = (data: RegisterForm) => {
    const lang = i18n.language === 'ar' ? Lang.Arabic : Lang.English;
    registerMutation.mutate({
      email: data.email,
      phoneNumber: data.phone,
      password: data.password,
      fullNameAr: data.nameAr,
      fullNameEn: data.nameEn,
      lang: lang
    }, {
      onSuccess: (res) => {
        setPhone(data.phone);
        setUserId(res.userId);
        setStep(2);
      },
      onError: (err: any) => {
        alert(err.response?.data?.detail || 'Registration failed. Please try again.');
      }
    });
  };

  const onVerify = (e: React.FormEvent) => {
    e.preventDefault();
    if (!otpCode) return;
    verifyOtpMutation.mutate({
      userId,
      code: otpCode,
    }, {
      onSuccess: () => {
        alert('Verification successful! You can now log in.');
        navigate('/login');
      },
      onError: (err: any) => {
        alert(err.response?.data?.detail || 'Invalid OTP. Please check the code.');
      }
    });
  };

  if (step === 2) {
    return (
      <div className="max-w-md mx-auto mt-10 p-6 bg-white rounded-xl shadow-sm border border-gray-100">
        <h2 className="text-2xl font-bold mb-4 text-center">{t('auth.verifyWhatsApp')}</h2>
        <p className="text-sm text-gray-600 text-center mb-6">
          We sent a code to {phone}
        </p>
        <form onSubmit={onVerify} className="space-y-4">
          <input 
            type="text" 
            value={otpCode}
            onChange={(e) => setOtpCode(e.target.value)}
            placeholder={t('auth.otpPlaceholder')}
            className="w-full border rounded-md px-3 py-2 text-center text-xl tracking-widest outline-none focus:ring-2 focus:ring-brand" 
          />
          <button 
            type="submit" 
            disabled={verifyOtpMutation.isPending}
            className="w-full bg-brand text-white py-2 rounded-md font-bold hover:bg-blue-600 disabled:opacity-50"
          >
            {verifyOtpMutation.isPending ? 'Verifying...' : 'Verify'}
          </button>
        </form>
      </div>
    );
  }

  return (
    <div className="max-w-md mx-auto mt-10 p-6 bg-white rounded-xl shadow-sm border border-gray-100">
      <h2 className="text-2xl font-bold mb-6 text-center">{t('auth.registerTitle')}</h2>
      
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-1">{t('auth.nameAr')}</label>
          <input {...register('nameAr')} dir="rtl" className="w-full border rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-brand" />
          {errors.nameAr && <p className="text-red-500 text-xs mt-1">{errors.nameAr.message}</p>}
        </div>
        
        <div>
          <label className="block text-sm font-medium mb-1">{t('auth.nameEn')}</label>
          <input {...register('nameEn')} dir="ltr" className="w-full border rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-brand" />
          {errors.nameEn && <p className="text-red-500 text-xs mt-1">{errors.nameEn.message}</p>}
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">{t('auth.email')}</label>
          <input type="email" {...register('email')} dir="ltr" className="w-full border rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-brand" />
          {errors.email && <p className="text-red-500 text-xs mt-1">{errors.email.message}</p>}
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">{t('auth.phone')}</label>
          <input type="tel" {...register('phone')} dir="ltr" className="w-full border rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-brand" />
          {errors.phone && <p className="text-red-500 text-xs mt-1">{errors.phone.message}</p>}
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">{t('auth.password')}</label>
          <input type="password" {...register('password')} dir="ltr" className="w-full border rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-brand" />
          {errors.password && <p className="text-red-500 text-xs mt-1">{errors.password.message}</p>}
        </div>

        <button 
          type="submit" 
          disabled={registerMutation.isPending}
          className="w-full bg-brand text-white py-2 rounded-md font-bold hover:bg-blue-600 transition disabled:opacity-50"
        >
          {registerMutation.isPending ? 'Registering...' : t('auth.submitRegister')}
        </button>
      </form>
    </div>
  );
}

