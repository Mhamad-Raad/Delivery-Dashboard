import { useTranslation } from 'react-i18next';
import { Check, Monitor, Moon, Sun, Palette, Type } from 'lucide-react';
import { useTheme } from '@/components/theme-provider';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { cn } from '@/lib/utils';

type ThemeOption = 'light' | 'dark' | 'system';
type ColorThemeOption = 'default' | 'claude' | 'cosmic-night' | 'supabase' | 'ocean-breeze' | 'aurora' | 'midnight-rose' | 'neon-cyber' | 'slate-storm';
type FontThemeOption = 'default' | 'modern' | 'classic' | 'mono' | 'rounded' | 'elegant' | 'compact' | 'playful' | 'times' | 'roboto' | 'opensans' | 'lato' | 'montserrat' | 'sourcecode' | 'literary' | 'thin';

interface ThemeCardProps {
  theme: ThemeOption;
  currentTheme: ThemeOption;
  onSelect: (theme: ThemeOption) => void;
  title: string;
  description: string;
  icon: React.ReactNode;
  preview: React.ReactNode;
}

interface ColorThemeCardProps {
  colorTheme: ColorThemeOption;
  currentColorTheme: ColorThemeOption;
  onSelect: (colorTheme: ColorThemeOption) => void;
  title: string;
  description: string;
  colors: { primary: string; secondary: string; accent: string };
}

interface FontThemeCardProps {
  fontTheme: FontThemeOption;
  currentFontTheme: FontThemeOption;
  onSelect: (fontTheme: FontThemeOption) => void;
  title: string;
  description: string;
  fontFamily: string;
  sampleText: string;
}

const ThemeCard = ({
  theme,
  currentTheme,
  onSelect,
  title,
  description,
  icon,
  preview,
}: ThemeCardProps) => {
  const isSelected = currentTheme === theme;

  return (
    <Card
      className={cn(
        'relative cursor-pointer transition-all duration-300 hover:shadow-lg hover:scale-[1.02]',
        isSelected
          ? 'ring-2 ring-primary shadow-lg'
          : 'hover:ring-1 hover:ring-primary/50'
      )}
      onClick={() => onSelect(theme)}
    >
      {isSelected && (
        <div className='absolute top-3 right-3 z-10'>
          <div className='bg-primary text-primary-foreground rounded-full p-1'>
            <Check className='size-4' />
          </div>
        </div>
      )}
      <CardHeader className='pb-3'>
        <div className='flex items-center gap-3'>
          <div
            className={cn(
              'p-2 rounded-lg transition-colors',
              isSelected ? 'bg-primary text-primary-foreground' : 'bg-muted'
            )}
          >
            {icon}
          </div>
          <div>
            <CardTitle className='text-lg'>{title}</CardTitle>
            <CardDescription className='text-sm'>{description}</CardDescription>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className='rounded-lg overflow-hidden border shadow-sm'>{preview}</div>
      </CardContent>
    </Card>
  );
};

const ColorThemeCard = ({
  colorTheme,
  currentColorTheme,
  onSelect,
  title,
  description,
  colors,
}: ColorThemeCardProps) => {
  const isSelected = currentColorTheme === colorTheme;

  return (
    <Card
      className={cn(
        'relative cursor-pointer transition-all duration-300 hover:shadow-lg hover:scale-[1.02]',
        isSelected
          ? 'ring-2 ring-primary shadow-lg'
          : 'hover:ring-1 hover:ring-primary/50'
      )}
      onClick={() => onSelect(colorTheme)}
    >
      {isSelected && (
        <div className='absolute top-3 right-3 z-10'>
          <div className='bg-primary text-primary-foreground rounded-full p-1'>
            <Check className='size-4' />
          </div>
        </div>
      )}
      <CardHeader className='pb-3'>
        <div className='flex items-center gap-3'>
          <div
            className={cn(
              'p-2 rounded-lg transition-colors',
              isSelected ? 'bg-primary text-primary-foreground' : 'bg-muted'
            )}
          >
            <Palette className='size-5' />
          </div>
          <div>
            <CardTitle className='text-lg'>{title}</CardTitle>
            <CardDescription className='text-sm'>{description}</CardDescription>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className='flex gap-2'>
          <div
            className='h-12 flex-1 rounded-lg shadow-sm border'
            style={{ backgroundColor: colors.primary }}
          />
          <div
            className='h-12 flex-1 rounded-lg shadow-sm border'
            style={{ backgroundColor: colors.secondary }}
          />
          <div
            className='h-12 flex-1 rounded-lg shadow-sm border'
            style={{ backgroundColor: colors.accent }}
          />
        </div>
      </CardContent>
    </Card>
  );
};

const FontThemeCard = ({
  fontTheme,
  currentFontTheme,
  onSelect,
  title,
  description,
  fontFamily,
  sampleText,
}: FontThemeCardProps) => {
  const isSelected = currentFontTheme === fontTheme;

  return (
    <Card
      className={cn(
        'relative cursor-pointer transition-all duration-300 hover:shadow-lg hover:scale-[1.02]',
        isSelected
          ? 'ring-2 ring-primary shadow-lg'
          : 'hover:ring-1 hover:ring-primary/50'
      )}
      onClick={() => onSelect(fontTheme)}
    >
      {isSelected && (
        <div className='absolute top-3 right-3 z-10'>
          <div className='bg-primary text-primary-foreground rounded-full p-1'>
            <Check className='size-4' />
          </div>
        </div>
      )}
      <CardHeader className='pb-3'>
        <div className='flex items-center gap-3'>
          <div
            className={cn(
              'p-2 rounded-lg transition-colors',
              isSelected ? 'bg-primary text-primary-foreground' : 'bg-muted'
            )}
          >
            <Type className='size-5' />
          </div>
          <div>
            <CardTitle className='text-lg'>{title}</CardTitle>
            <CardDescription className='text-sm'>{description}</CardDescription>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div
          className='p-4 rounded-lg border bg-muted/30 text-center'
          style={{ fontFamily }}
        >
          <p className='text-2xl font-semibold mb-1'>{sampleText}</p>
          <p className='text-sm text-muted-foreground'>Aa Bb Cc 123</p>
        </div>
      </CardContent>
    </Card>
  );
};

// Theme preview components
const LightPreview = () => (
  <div className='bg-white p-3 space-y-2'>
    <div className='flex items-center gap-2'>
      <div className='w-8 h-8 rounded bg-gray-100' />
      <div className='flex-1 space-y-1'>
        <div className='h-2 bg-gray-200 rounded w-3/4' />
        <div className='h-2 bg-gray-100 rounded w-1/2' />
      </div>
    </div>
    <div className='grid grid-cols-3 gap-2'>
      <div className='h-8 rounded bg-gray-50 border border-gray-200' />
      <div className='h-8 rounded bg-black' />
      <div className='h-8 rounded bg-gray-100' />
    </div>
    <div className='space-y-1'>
      <div className='h-2 bg-gray-200 rounded' />
      <div className='h-2 bg-gray-100 rounded w-5/6' />
    </div>
  </div>
);

const DarkPreview = () => (
  <div className='bg-zinc-950 p-3 space-y-2'>
    <div className='flex items-center gap-2'>
      <div className='w-8 h-8 rounded bg-zinc-800' />
      <div className='flex-1 space-y-1'>
        <div className='h-2 bg-zinc-700 rounded w-3/4' />
        <div className='h-2 bg-zinc-800 rounded w-1/2' />
      </div>
    </div>
    <div className='grid grid-cols-3 gap-2'>
      <div className='h-8 rounded bg-zinc-900 border border-zinc-700' />
      <div className='h-8 rounded bg-white' />
      <div className='h-8 rounded bg-zinc-800' />
    </div>
    <div className='space-y-1'>
      <div className='h-2 bg-zinc-700 rounded' />
      <div className='h-2 bg-zinc-800 rounded w-5/6' />
    </div>
  </div>
);

const SystemPreview = () => (
  <div className='grid grid-cols-2 overflow-hidden'>
    <div className='bg-white p-2 space-y-1.5'>
      <div className='flex items-center gap-1.5'>
        <div className='w-5 h-5 rounded bg-gray-100' />
        <div className='flex-1 space-y-0.5'>
          <div className='h-1.5 bg-gray-200 rounded w-3/4' />
          <div className='h-1.5 bg-gray-100 rounded w-1/2' />
        </div>
      </div>
      <div className='h-5 rounded bg-black' />
      <div className='h-1.5 bg-gray-200 rounded' />
    </div>
    <div className='bg-zinc-950 p-2 space-y-1.5'>
      <div className='flex items-center gap-1.5'>
        <div className='w-5 h-5 rounded bg-zinc-800' />
        <div className='flex-1 space-y-0.5'>
          <div className='h-1.5 bg-zinc-700 rounded w-3/4' />
          <div className='h-1.5 bg-zinc-800 rounded w-1/2' />
        </div>
      </div>
      <div className='h-5 rounded bg-white' />
      <div className='h-1.5 bg-zinc-700 rounded' />
    </div>
  </div>
);

const Themes = () => {
  const { t } = useTranslation('themes');
  const { theme, setTheme, colorTheme, setColorTheme, fontTheme, setFontTheme } = useTheme();

  const themeOptions: {
    theme: ThemeOption;
    titleKey: string;
    descriptionKey: string;
    icon: React.ReactNode;
    preview: React.ReactNode;
  }[] = [
    {
      theme: 'light',
      titleKey: 'lightTheme',
      descriptionKey: 'lightThemeDescription',
      icon: <Sun className='size-5' />,
      preview: <LightPreview />,
    },
    {
      theme: 'dark',
      titleKey: 'darkTheme',
      descriptionKey: 'darkThemeDescription',
      icon: <Moon className='size-5' />,
      preview: <DarkPreview />,
    },
    {
      theme: 'system',
      titleKey: 'systemTheme',
      descriptionKey: 'systemThemeDescription',
      icon: <Monitor className='size-5' />,
      preview: <SystemPreview />,
    },
  ];

  const colorThemeOptions: {
    colorTheme: ColorThemeOption;
    titleKey: string;
    descriptionKey: string;
    colors: { primary: string; secondary: string; accent: string };
  }[] = [
    {
      colorTheme: 'default',
      titleKey: 'defaultColorTheme',
      descriptionKey: 'defaultColorThemeDescription',
      colors: {
        primary: '#000000',
        secondary: '#f0f0f0',
        accent: '#e5e5e5',
      },
    },
    {
      colorTheme: 'claude',
      titleKey: 'claudeColorTheme',
      descriptionKey: 'claudeColorThemeDescription',
      colors: {
        primary: '#b45309',
        secondary: '#fef3c7',
        accent: '#fde68a',
      },
    },
    {
      colorTheme: 'cosmic-night',
      titleKey: 'cosmicNightColorTheme',
      descriptionKey: 'cosmicNightColorThemeDescription',
      colors: {
        primary: '#7c3aed',
        secondary: '#e0e7ff',
        accent: '#c4b5fd',
      },
    },
    {
      colorTheme: 'supabase',
      titleKey: 'supabaseColorTheme',
      descriptionKey: 'supabaseColorThemeDescription',
      colors: {
        primary: '#3ecf8e',
        secondary: '#f0f0f0',
        accent: '#b8f0d8',
      },
    },
    {
      colorTheme: 'ocean-breeze',
      titleKey: 'oceanBreezeColorTheme',
      descriptionKey: 'oceanBreezeColorThemeDescription',
      colors: {
        primary: '#2dd4bf',
        secondary: '#e0f2fe',
        accent: '#99f6e4',
      },
    },
    {
      colorTheme: 'aurora',
      titleKey: 'auroraColorTheme',
      descriptionKey: 'auroraColorThemeDescription',
      colors: {
        primary: '#38bdf8',
        secondary: '#e0e7ff',
        accent: '#f0abfc',
      },
    },
    {
      colorTheme: 'midnight-rose',
      titleKey: 'midnightRoseColorTheme',
      descriptionKey: 'midnightRoseColorThemeDescription',
      colors: {
        primary: '#e11d48',
        secondary: '#ddd6fe',
        accent: '#fda4af',
      },
    },
    {
      colorTheme: 'neon-cyber',
      titleKey: 'neonCyberColorTheme',
      descriptionKey: 'neonCyberColorThemeDescription',
      colors: {
        primary: '#ec4899',
        secondary: '#67e8f9',
        accent: '#22d3d1',
      },
    },
    {
      colorTheme: 'slate-storm',
      titleKey: 'slateStormColorTheme',
      descriptionKey: 'slateStormColorThemeDescription',
      colors: {
        primary: '#64748b',
        secondary: '#cbd5e1',
        accent: '#94a3b8',
      },
    },
  ];

  const fontThemeOptions: {
    fontTheme: FontThemeOption;
    titleKey: string;
    descriptionKey: string;
    fontFamily: string;
    sampleText: string;
  }[] = [
    {
      fontTheme: 'default',
      titleKey: 'defaultFontTheme',
      descriptionKey: 'defaultFontThemeDescription',
      fontFamily: 'ui-sans-serif, system-ui, sans-serif',
      sampleText: 'The quick fox',
    },
    {
      fontTheme: 'modern',
      titleKey: 'modernFontTheme',
      descriptionKey: 'modernFontThemeDescription',
      fontFamily: '"Inter", ui-sans-serif, system-ui, sans-serif',
      sampleText: 'The quick fox',
    },
    {
      fontTheme: 'classic',
      titleKey: 'classicFontTheme',
      descriptionKey: 'classicFontThemeDescription',
      fontFamily: 'Georgia, "Times New Roman", serif',
      sampleText: 'The quick fox',
    },
    {
      fontTheme: 'mono',
      titleKey: 'monoFontTheme',
      descriptionKey: 'monoFontThemeDescription',
      fontFamily: '"JetBrains Mono", "Fira Code", monospace',
      sampleText: 'The quick fox',
    },
    {
      fontTheme: 'rounded',
      titleKey: 'roundedFontTheme',
      descriptionKey: 'roundedFontThemeDescription',
      fontFamily: '"Nunito", sans-serif',
      sampleText: 'The quick fox',
    },
    {
      fontTheme: 'elegant',
      titleKey: 'elegantFontTheme',
      descriptionKey: 'elegantFontThemeDescription',
      fontFamily: '"Playfair Display", serif',
      sampleText: 'The quick fox',
    },
    {
      fontTheme: 'compact',
      titleKey: 'compactFontTheme',
      descriptionKey: 'compactFontThemeDescription',
      fontFamily: '"IBM Plex Sans", sans-serif',
      sampleText: 'The quick fox',
    },
    {
      fontTheme: 'playful',
      titleKey: 'playfulFontTheme',
      descriptionKey: 'playfulFontThemeDescription',
      fontFamily: '"Poppins", sans-serif',
      sampleText: 'The quick fox',
    },
    {
      fontTheme: 'times',
      titleKey: 'timesFontTheme',
      descriptionKey: 'timesFontThemeDescription',
      fontFamily: '"Times New Roman", Times, serif',
      sampleText: 'The quick fox',
    },
    {
      fontTheme: 'roboto',
      titleKey: 'robotoFontTheme',
      descriptionKey: 'robotoFontThemeDescription',
      fontFamily: '"Roboto", sans-serif',
      sampleText: 'The quick fox',
    },
    {
      fontTheme: 'opensans',
      titleKey: 'opensansFontTheme',
      descriptionKey: 'opensansFontThemeDescription',
      fontFamily: '"Open Sans", sans-serif',
      sampleText: 'The quick fox',
    },
    {
      fontTheme: 'lato',
      titleKey: 'latoFontTheme',
      descriptionKey: 'latoFontThemeDescription',
      fontFamily: '"Lato", sans-serif',
      sampleText: 'The quick fox',
    },
    {
      fontTheme: 'montserrat',
      titleKey: 'montserratFontTheme',
      descriptionKey: 'montserratFontThemeDescription',
      fontFamily: '"Montserrat", sans-serif',
      sampleText: 'The quick fox',
    },
    {
      fontTheme: 'sourcecode',
      titleKey: 'sourcecodeFontTheme',
      descriptionKey: 'sourcecodeFontThemeDescription',
      fontFamily: '"Source Code Pro", monospace',
      sampleText: 'The quick fox',
    },
    {
      fontTheme: 'literary',
      titleKey: 'literaryFontTheme',
      descriptionKey: 'literaryFontThemeDescription',
      fontFamily: '"Merriweather", serif',
      sampleText: 'The quick fox',
    },
    {
      fontTheme: 'thin',
      titleKey: 'thinFontTheme',
      descriptionKey: 'thinFontThemeDescription',
      fontFamily: '"Raleway", sans-serif',
      sampleText: 'The quick fox',
    },
  ];

  return (
    <div className='p-6 space-y-8'>
      <div>
        <h1 className='text-3xl font-bold tracking-tight'>{t('title')}</h1>
        <p className='text-muted-foreground mt-1'>{t('subtitle')}</p>
      </div>

      {/* Appearance Mode Section */}
      <div className='space-y-4'>
        <div>
          <h2 className='text-xl font-semibold'>{t('appearanceTitle')}</h2>
          <p className='text-muted-foreground text-sm'>{t('appearanceSubtitle')}</p>
        </div>
        <div className='grid gap-6 md:grid-cols-2 lg:grid-cols-3'>
          {themeOptions.map((option) => (
            <ThemeCard
              key={option.theme}
              theme={option.theme}
              currentTheme={theme}
              onSelect={setTheme}
              title={t(option.titleKey)}
              description={t(option.descriptionKey)}
              icon={option.icon}
              preview={option.preview}
            />
          ))}
        </div>
      </div>

      {/* Color Theme Section */}
      <div className='space-y-4'>
        <div>
          <h2 className='text-xl font-semibold'>{t('colorThemeTitle')}</h2>
          <p className='text-muted-foreground text-sm'>{t('colorThemeSubtitle')}</p>
        </div>
        <div className='grid gap-6 md:grid-cols-2 lg:grid-cols-3'>
          {colorThemeOptions.map((option) => (
            <ColorThemeCard
              key={option.colorTheme}
              colorTheme={option.colorTheme}
              currentColorTheme={colorTheme}
              onSelect={setColorTheme}
              title={t(option.titleKey)}
              description={t(option.descriptionKey)}
              colors={option.colors}
            />
          ))}
        </div>
      </div>

      {/* Font Theme Section */}
      <div className='space-y-4'>
        <div>
          <h2 className='text-xl font-semibold'>{t('fontThemeTitle')}</h2>
          <p className='text-muted-foreground text-sm'>{t('fontThemeSubtitle')}</p>
        </div>
        <div className='grid gap-6 md:grid-cols-2 lg:grid-cols-4'>
          {fontThemeOptions.map((option) => (
            <FontThemeCard
              key={option.fontTheme}
              fontTheme={option.fontTheme}
              currentFontTheme={fontTheme}
              onSelect={setFontTheme}
              title={t(option.titleKey)}
              description={t(option.descriptionKey)}
              fontFamily={option.fontFamily}
              sampleText={option.sampleText}
            />
          ))}
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{t('moreThemesTitle')}</CardTitle>
          <CardDescription>{t('moreThemesDescription')}</CardDescription>
        </CardHeader>
      </Card>
    </div>
  );
};

export default Themes;
