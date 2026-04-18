import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { useTheme } from '@/components/theme-provider';
import { useTranslation } from 'react-i18next';
import { Monitor, Moon, Sun } from 'lucide-react';

const AppearanceSettings = () => {
  const { theme, setTheme } = useTheme();
  const { i18n } = useTranslation();

  const themes = [
    { value: 'light', label: 'Light', icon: Sun },
    { value: 'dark', label: 'Dark', icon: Moon },
    { value: 'system', label: 'System', icon: Monitor },
  ];

  const languages = [
    { code: 'en', name: 'English' },
    { code: 'ar', name: 'Arabic (العربية)' },
    { code: 'ku', name: 'Kurdish (کوردی)' },
  ];

  return (
    <div className='space-y-6'>
      {/* Theme Settings */}
      <Card>
        <CardHeader>
          <CardTitle>Theme</CardTitle>
          <CardDescription>
            Select your preferred color theme for the dashboard
          </CardDescription>
        </CardHeader>
        <CardContent className='space-y-4'>
          <div className='grid grid-cols-1 md:grid-cols-3 gap-4'>
            {themes.map((themeOption) => {
              const Icon = themeOption.icon;
              return (
                <button
                  key={themeOption.value}
                  onClick={() => setTheme(themeOption.value as 'light' | 'dark' | 'system')}
                  className={`flex flex-col items-center justify-center gap-3 p-6 rounded-lg border-2 transition-all ${
                    theme === themeOption.value
                      ? 'border-primary bg-primary/5'
                      : 'border-border hover:border-primary/50'
                  }`}
                >
                  <Icon className='h-8 w-8' />
                  <span className='font-medium'>{themeOption.label}</span>
                </button>
              );
            })}
          </div>
        </CardContent>
      </Card>

      {/* Language Settings */}
      <Card>
        <CardHeader>
          <CardTitle>Language</CardTitle>
          <CardDescription>
            Choose your preferred language for the interface
          </CardDescription>
        </CardHeader>
        <CardContent className='space-y-4'>
          <div className='grid grid-cols-1 gap-3'>
            {languages.map((language) => (
              <button
                key={language.code}
                onClick={() => {
                  i18n.changeLanguage(language.code);
                  localStorage.setItem('locale', language.code);
                }}
                className={`flex items-center justify-between p-4 rounded-lg border-2 transition-all ${
                  i18n.language === language.code
                    ? 'border-primary bg-primary/5'
                    : 'border-border hover:border-primary/50'
                }`}
              >
                <span className='font-medium'>{language.name}</span>
                {i18n.language === language.code && (
                  <div className='h-2 w-2 rounded-full bg-primary' />
                )}
              </button>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Display Settings */}
      <Card>
        <CardHeader>
          <CardTitle>Display Preferences</CardTitle>
          <CardDescription>
            Customize how information is displayed
          </CardDescription>
        </CardHeader>
        <CardContent className='space-y-4'>
          <div className='flex items-center justify-between'>
            <div className='space-y-1'>
              <Label>Compact Mode</Label>
              <p className='text-sm text-muted-foreground'>
                Display more content by reducing spacing
              </p>
            </div>
            <Button variant='outline' size='sm'>
              Toggle
            </Button>
          </div>

          <div className='flex items-center justify-between'>
            <div className='space-y-1'>
              <Label>Show Sidebar by Default</Label>
              <p className='text-sm text-muted-foreground'>
                Keep the navigation sidebar expanded on page load
              </p>
            </div>
            <Button variant='outline' size='sm'>
              Toggle
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default AppearanceSettings;
