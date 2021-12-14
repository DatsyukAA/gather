FROM node:12.18.1

ENV NODE_ENV=production
ENV jwt_secret=vPQ64lbYKw6S7hRaU1HHDqmDBtrrBicN
ENV DATABASE=mongodb://localhost:27017/

WORKDIR /app
COPY ["package.json" "yarn.lock*", "./"]

RUN npm install -g yarn
RUN yarn --production

COPY . .

CMD ["yarn", "start"]