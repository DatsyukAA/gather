export interface IEntity {
    id: string;
    creationDate: Date;
}

export class Entity implements IEntity {

    id: string;
    creationDate: Date;

    constructor(_id: string | number) {
        this.id = _id.toString();
        this.creationDate = new Date(Date.now());
    }
}