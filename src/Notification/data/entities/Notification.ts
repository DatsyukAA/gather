import { Entity } from '../Entity'
export default class Notification extends Entity {
    creationDate: Date;
    title: string;
    text: string;
    sender: string;
    constructor(_id: number, _sender: string, _creationDate: Date, _title: string, _text: string) {
        super(_id);

        this.creationDate = _creationDate;
        this.title = _title;
        this.text = _text;
        this.sender = _sender;
    }
}