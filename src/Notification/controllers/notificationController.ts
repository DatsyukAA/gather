import express from 'express';
import INotificationService from '../services/INotificationService';
import Notification from '../data/entities/Notification';

export default function (app: express.Express, notificationService: INotificationService) {
    app.get('/', (req, res) => {
        let subAction = (notification: Notification) => {
            res.send(`${notification.id} ${notification.creationDate} ${notification.sender}\n ${notification.title} \n ${notification.text} `);
        };
        notificationService.subscribe(subAction, req.query.id as string)
    });
}
