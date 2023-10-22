FROM redis:7.2.2

COPY redis.conf /usr/local/etc/redis/
COPY background.sh /usr/local/etc/redis/
COPY wait-init-user.sh /usr/local/etc/redis/

RUN chmod 777 /usr/local/etc/redis/
RUN chmod 777 /usr/local/etc/redis/background.sh
RUN chmod 777 /usr/local/etc/redis/wait-init-user.sh

CMD /usr/local/etc/redis/background.sh && redis-server /usr/local/etc/redis/redis.conf