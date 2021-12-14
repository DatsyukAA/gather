import express from "express";
import requestLogger from "./middlewares/requestLogger";
import notificationController from "./controllers/notificationController";
import { MongoClient } from "mongodb";
import bodyParser from "./middlewares/bodyParser";
import { config } from "./config";
import * as WebSocket from "ws";
import * as http from "http";
import WSHandler from "./websocket/WSHandler"
import { NotificationService } from "./services/Impl/NotificationService";
import NotificationRepository from "./data/repos/NotificationRepository";
import amqp from 'amqplib/callback_api'


const app = express();
const server = http.createServer(app);

function withAMQP(callback: Function) {
    amqp.connect('amqp://localhost', (err, connection) => {
        if (err) throw err;
        connection.createChannel((chErr, channel) => {
            if (chErr) throw chErr;
            callback(channel)
        })
    });
}

type MessageCallback = (message: amqp.Message | null) => void;

function subscribeExchange(channel: amqp.Channel, queueName: string, action: MessageCallback, exchangeName: string = '', exchangeType: string = 'direct', queueBindPattern: string = '') {
    channel.assertExchange(exchangeName, exchangeType, {
        durable: false
    }, (errExchange, exchangeResult) => {
        if (errExchange) throw errExchange;
        subscribeQueue(channel, queueName, action, true, exchangeResult.exchange, queueBindPattern)
    })
}

function subscribeQueue(channel: amqp.Channel, queueName: string, action: MessageCallback, bindExchange: boolean = false, exchangeName: string = '', queueBindPattern: string = '') {
    channel.assertQueue(queueName, {
        exclusive: true
    }, (errQueue, queueResult) => {
        if (errQueue) throw errQueue;
        if (bindExchange)
            channel.bindQueue(queueResult.queue, exchangeName, queueBindPattern)

        channel.consume(queueResult.queue, action, {
            noAck: true
        })
    });
}

function withMongo(callback: Function) {
    MongoClient.connect(
        config.mongo.connectionStrings.default ?? process.env.DATABASE, {},
        (err, mongodb) => {
            if (err) throw err;
            if (mongodb == undefined) throw new Error('mongodb undefined');
            callback(mongodb)
        }
    );
}

const middlewares = [requestLogger, bodyParser];
const controllers = [notificationController];
middlewares.forEach((element) => {
    app.use(element);
});


const wss = new WebSocket.Server({
    server
});


withMongo((mongodb: MongoClient) => {
    const notificationRepos = new NotificationRepository(mongodb.db(config.mongo.dbName || process.env.DATABASE), 'notifications');
    const notificationService = new NotificationService(notificationRepos);

    withAMQP((channel: amqp.Channel) => {
        subscribeExchange(channel, 'notificationQueue', (message: amqp.Message | null) => {
            let messageObj = JSON.parse(message?.content?.toString() ?? '');
            Object.keys(messageObj).forEach(key => {
                console.log(key, messageObj[key]);
            });
        }, 'notifications')
    })
    controllers.forEach((element) => {
        element(app, notificationService);
    });

    // websocket setup
    new WSHandler(wss, notificationService)
    // http setup
    server.listen(process.env.PORT || config.port, () => {
        return console.log(`server started on ${config.port}...`);
    });
})
