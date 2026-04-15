import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

import en from './locales/en.json';
import ar from './locales/ar.json';
import ku from './locales/ku.json';

import sidebarEn from './locales/Sidebar/en.json';
import sidebarAr from './locales/Sidebar/ar.json';
import sidebarKu from './locales/Sidebar/ku.json';

import navbarEn from './locales/navbar/en.json';
import navbarAr from './locales/navbar/ar.json';
import navbarKu from './locales/navbar/ku.json';

import themesEn from './locales/Themes/en.json';
import themesAr from './locales/Themes/ar.json';
import themesKu from './locales/Themes/ku.json';

i18n.use(initReactI18next).init({
  resources: {
    en: {
      translation: en,
      sidebar: sidebarEn,
      navbar: navbarEn,
      themes: themesEn,
    },
    ar: {
      translation: ar,
      sidebar: sidebarAr,
      navbar: navbarAr,
      themes: themesAr,
    },
    ku: {
      translation: ku,
      sidebar: sidebarKu,
      navbar: navbarKu,
      themes: themesKu,
    },
  },
  lng: 'en', // default language
  fallbackLng: 'en',
  interpolation: { escapeValue: false },
});

export default i18n;
