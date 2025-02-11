namespace SODERIA_I.wwwroot {
    // sw.js - Service Worker básico
    self.addEventListener('install', event => {
        console.log('Service Worker: Installed');
        event.waitUntil(
            caches.open('static-v1').then(cache => {
                return cache.addAll([
                    '/',
                    '/css/site.css',      // Ajustá según los archivos estáticos que usás
                    '/js/site.js',
                    '/bidon.png'
                    // Agrega aquí más archivos que quieras cachear
                ]);
            })
        );
        self.skipWaiting();
    });

    self.addEventListener('activate', event => {
        console.log('Service Worker: Activated');
        // Aquí podés limpiar caches viejos si lo necesitás
    });

    self.addEventListener('fetch', event => {
        event.respondWith(
            caches.match(event.request).then(response => {
                return response || fetch(event.request);
            })
        );
        self.clients.claim();
    });
}
