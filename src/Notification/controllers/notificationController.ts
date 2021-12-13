import express from 'express';
import INotificationService from '../services/INotificationService';
import Notification from '../data/entities/Notification';

export default function (app: express.Express, notificationService: INotificationService) {
    app.get('/', (req, res) => {
        var subAction = (notification: Notification) => {
            res.send(`${notification.id} ${notification.creationDate} ${notification.sender}\n ${notification.title} \n ${notification.text} `);
        };
        notificationService.subscribe(subAction, req.query.id as string)
    })
    app.get('/notify', (req, res) => {
        setTimeout(() => {
            notificationService.notify(new Notification('1', 'Test Title', 'Test notifications'));
        }, 10000)
        res.send('ok')
    })
}
