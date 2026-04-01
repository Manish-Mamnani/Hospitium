import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  {
    path: 'hotels/:id',
    renderMode: RenderMode.Prerender,
    getPrerenderParams: async () => {
      // Replace these IDs with your real hotel IDs from backend if needed.
      return [
        { id: '1' },
        { id: '2' },
        { id: '3' }
      ];
    }
  },
  {
    path: '**',
    renderMode: RenderMode.Prerender
  }
];
