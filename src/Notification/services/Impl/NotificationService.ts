import Notification from "../../data/entities/Notification";
import IRepository from "../../data/IRepository";
import INotificationService from "../INotificationService";

export class NotificationService implements INotificationService {

    private _callbacks: any = {};
    private _notificationRepo: IRepository<Notification>;
    constructor(notificationRepo: IRepository<Notification>) {
        this._notificationRepo = notificationRepo;
    }
    subscribe(callback: Function, channelId: string = 'all'): void {
        if (!this._callbacks[channelId]) this._callbacks[channelId] = [];
        (this._callbacks[channelId] as Function[]).push(callback)
    }

    unsubscribe(callback: Function, channelId: string = 'all'): void {
        if (channelId && !this._callbacks[channelId]) return;
        if (channelId)
            (this._callbacks[channelId] as Function[]).filter(x => x != callback)
    }
    notify(notification: Notification, channelId: string = 'all'): void {
        this._notificationRepo.Insert(notification);
        if (channelId !== 'all') {
            this._callbacks[channelId].forEach((element: Function) => {
                element(notification);
            });
            this._callbacks[channelId] = []
        } else {
            Object.keys(this._callbacks).forEach(channelId => {
                this._callbacks[channelId].forEach((element: (notification: Notification) => void) => {
                    element(notification);
                });
                this._callbacks[channelId] = []
            })
        }
    }
}