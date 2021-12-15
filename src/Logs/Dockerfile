FROM node:12.18.1

ENV NODE_ENV=production

WORKDIR /app
COPY ["package.json" "yarn.lock*", "./"]

RUN npm install -g yarn
RUN yarn --production

COPY . .

CMD ["yarn", "start"]