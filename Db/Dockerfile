FROM mcr.microsoft.com/mssql/server:2019-latest

USER root

RUN touch /var/log/cron.log

WORKDIR /app

COPY . .

RUN chmod +x /app/task.sh && chmod +x /app/entrypoint.sh
RUN apt-get update && apt-get -y install cron

ADD crontab /etc/cron.d/crontab
RUN chmod 0644 /etc/cron.d/crontab && crontab /etc/cron.d/crontab

ENTRYPOINT ["./entrypoint.sh"]