const API_URL = '/api/books'; // Nginx будет проксировать /api на .NET бэкенд

async function loadBooks() {
  const container = document.getElementById('books-list');
  const badge = document.getElementById('status-badge');

  try {
    const response = await fetch(API_URL);

    if (!response.ok) {
      throw new Error(`Ошибка сервера: ${response.status}`);
    }

    const books = await response.json();
    
    badge.className = 'badge badge-outline badge-success';
    badge.textContent = 'API: Online';

    if (!books || books.length === 0) {
      container.innerHTML = `
        <div class="col-span-full text-center py-12 text-base-content/60">
          Книг пока нет. Создайте первую через API!
        </div>`;
      return;
    }

    container.innerHTML = books.map(book => `
      <div class="card bg-base-100 shadow-xl border border-base-200 hover:border-primary transition-all">
        <div class="card-body">
          <h2 class="card-title text-primary">${escapeHtml(book.title || 'Без названия')}</h2>
          <p class="text-sm text-base-content/70">Автор ID: ${escapeHtml(book.authorId || 'Не указан')}</p>
          <div class="card-actions justify-end mt-4">
            <div class="badge badge-secondary">${escapeHtml(book.genre || 'Общее')}</div>
          </div>
        </div>
      </div>
    `).join('');

  } catch (error) {
    console.error('Fetch error:', error);
    badge.className = 'badge badge-outline badge-error';
    badge.textContent = 'API: Offline';
    
    container.innerHTML = `
      <div class="col-span-full alert alert-error shadow-lg">
        <span>Не удалось загрузить данные с бэкенда. Проверьте соединение с API.</span>
      </div>`;
  }
}

function escapeHtml(string) {
  return String(string).replace(/[&<>"']/g, (s) => ({
    '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
  })[s]);
}

// Первичная загрузка при открытии страницы
document.addEventListener('DOMContentLoaded', loadBooks);