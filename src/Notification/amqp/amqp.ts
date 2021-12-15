import amqp from 'amqplib/callback_api'
import { config } from '../config';

const withAMQP = () => new Promise<amqp.Channel>((resolve, reject) => {
    amqp.connect(config.rabbit.host ?? process.env.RabbitHost, (err, connection) => {
        if (err) reject(err);
        connection.createChannel((chErr, channel) => {
            if (chErr || !channel) reject(chErr ?? 'channel is null');
            resolve(channel)
        })
    });
});

const subscribeExchange = (channel: amqp.Channel, exchangeName: string, exchangeType: string = 'direct') =>
    new Promise<{ exchange: string, channel: amqp.Channel }>((resolve, reject) => {
        channel.assertExchange(exchangeName, exchangeType, {
            durable: false
        }, (errExchange, exchangeResult) => {
            if (errExchange) reject(errExchange);
            resolve({ exchange: exchangeResult.exchange, channel })
        })
    });
const subscribeQueue = (channel: amqp.Channel, queueName: string, exchangeName?: string, queueBindPattern?: string) =>
    new Promise<{ queue: string, channel: amqp.Channel }>((resolve, reject) => {
        channel.assertQueue(queueName, {}, (errQueue, queueResult) => {
            if (errQueue) reject(errQueue);
            if (!!exchangeName)
                channel.bindQueue(queueResult.queue, exchangeName, queueBindPattern ?? '')
            resolve({ queue: queueResult.queue, channel })
        });
    });

const initRabbitChannel = (queueName: string, exchangeName?: string) => new Promise<{ channel: amqp.Channel, queue: string, exchange?: string }>((resolve, reject) => {
    withAMQP().then((channel: amqp.Channel) => {
        if (!!exchangeName)
            subscribeExchange(channel, exchangeName).then((exchangeResult) => {
                subscribeQueue(channel, queueName, exchangeResult.exchange).then(queueResult => {
                    resolve({ channel, queue: queueResult.queue, exchange: exchangeResult.exchange })
                });
            });
        else subscribeQueue(channel, queueName).then(queueResult => {
            resolve({ channel, queue: queueResult.queue })
        });
    }).catch(err => {
        reject(err);
    });
})

export const notifications = initRabbitChannel('notificationsQueue', 'notifications')