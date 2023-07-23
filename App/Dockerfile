FROM mcr.microsoft.com/dotnet/sdk:3.1

ENV BotToken unknown
ENV DbConn unknown

RUN mkdir -p /data

COPY ./Data/shelters.csv /data

WORKDIR /app

COPY . .

ADD ./Config/nlog.config ./nlog.config

RUN dotnet publish "SearchSheltersBot.csproj" -c Release -o /out

ADD ./Db/Scripts/init.sql /out
ADD ./Config/nlog.config /out

WORKDIR /out

ENTRYPOINT ["dotnet","SearchSheltersBot.dll"]
