import amqp from 'amqplib/callback_api'
import fs from 'fs'

const withAMQP = () => new Promise<amqp.Channel>((resolve, reject) => {
    amqp.connect(process.env.RabbitHost ?? 'amqp://localhost', (err, connection) => {
        if (err) reject(err);
        connection.createChannel((chErr, channel) => {
            if (chErr || !channel) reject(chErr ?? 'channel is null');
            resolve(channel)
        })
    });
});

const subscribeExchange = (channel: amqp.Channel, exchangeName: string, exchangeType: string = 'direct') => new Promise<string>((resolve, reject) => {
    channel.assertExchange(exchangeName, exchangeType, {
        durable: false
    }, (errExchange, exchangeResult) => {
        if (errExchange) reject(errExchange);
        resolve(exchangeResult.exchange)
    })
});
const subscribeQueue = (channel: amqp.Channel, queueName: string, action: (message: amqp.Message | null) => void, exchangeName?: string, queueBindPattern?: string) => new Promise((resolve, reject) => {
    channel.assertQueue(queueName, {}, (errQueue, queueResult) => {
        if (errQueue) reject(errQueue);
        if (!!exchangeName)
            channel.bindQueue(queueResult.queue, exchangeName, queueBindPattern ?? '')
        channel.consume(queueResult.queue, action)
    });
});

const subscribeLog = (channel: amqp.Channel, message: amqp.Message, logLevel: string) => {
    let messageObj = JSON.parse(message?.content?.toString() ?? '');
    let dir = `${process.env.LogDir ?? './logs'}/${messageObj.Sender}`.replace(':', '_');
    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true });
    }
    fs.writeFile(`${dir}/${logLevel}.log`, `[${messageObj.Time}] ${messageObj.Message}\n`, { flag: 'a+' }, err => {
        if (err) {
            console.error(err)
            return
        }
        channel.ack(message);
    })
    console.log(`[${messageObj.Time}][${logLevel}] ${messageObj.Message}`);
}

withAMQP().then((channel: amqp.Channel) => {
    subscribeExchange(channel, 'logs').then((exchange) => {
        subscribeQueue(channel, 'notifications', (message) => {
            console.log(message?.fields);
            let messageObj = JSON.parse(message?.content?.toString() ?? '');
            Object.keys(messageObj).forEach(key => {
                console.log(key, messageObj[key]);
            });
        }, exchange);

        subscribeQueue(channel, 'logDebug', (message) => {
            if (!!message) console.log('message is null')
            if (message)
                subscribeLog(channel, message, 'debug')
        }, exchange, 'debug');

        subscribeQueue(channel, 'logInfo', (message) => {
            if (!!message) console.log('message is null')
            if (message)
                subscribeLog(channel, message, 'info')
        }, exchange, 'information');

        subscribeQueue(channel, 'logWarn', (message) => {
            if (!!message) console.log('message is null')
            if (message)
                subscribeLog(channel, message, 'warn')
        }, exchange, 'warning');

        subscribeQueue(channel, 'logErr', (message) => {
            if (!!message) console.log('message is null')
            if (message)
                subscribeLog(channel, message, 'error')
        }, exchange, 'error');

        subscribeQueue(channel, 'logCrit', (message) => {
            if (!!message) console.log('message is null')
            if (message)
                subscribeLog(channel, message, 'critical')
        }, exchange, 'critical');
    });
});