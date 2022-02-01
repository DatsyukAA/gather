import Notification from "../../data/entities/Notification";
import IRepository from "../../data/IRepository";
import INotificationService from "../INotificationService";

export class NotificationService implements INotificationService {

    private _callbacks: any = {};
    private _notificationRepo: IRepository<Notification>;
    constructor(notificationRepo: IRepository<Notification>) {
        this._notificationRepo = notificationRepo;
    }
    subscribe(callback: (notification: Notification) => void, channelId: string = 'public'): void {
        if (!this._callbacks[channelId]) this._callbacks[channelId] = [];
        (this._callbacks[channelId] as Function[]).push(callback)
    }

    unsubscribe(callback: (notification: Notification) => void, channelId: string = 'public'): void {
        if (channelId && !this._callbacks[channelId]) return;
        if (channelId)
            (this._callbacks[channelId] as ((notification: Notification) => void)[]).filter(x => x != callback)
    }
    notify(notification: Notification, channelId: string = 'public'): void {
        this._notificationRepo.Insert(notification);
        if (channelId !== 'public') {
            this._callbacks[channelId].map((element: (notification: Notification) => void) => {
                try {
                    element(notification);
                    return true;
                } catch {
                    return false;
                }
            });
        } else {
            Object.keys(this._callbacks).forEach(channelId => {
                this._callbacks[channelId].map((element: (notification: Notification) => void) => {
                    try {
                        element(notification);
                        return true;
                    } catch {
                        return false;
                    }
                });
            })
        }
    }
}