# Берем легкий образ Nginx
FROM nginx:alpine

# Удаляем дефолтный конфиг
RUN rm /etc/nginx/conf.d/default.conf

# Копируем наш новый конфиг
COPY nginx/nginx.conf /etc/nginx/nginx.conf

# Копируем папку frontend с твоей версткой в директорию раздачи Nginx
COPY frontend/ /usr/share/nginx/html/