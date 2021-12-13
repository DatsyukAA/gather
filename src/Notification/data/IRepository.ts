export default interface IRepository<TEntity> {
    Single(predicate: any): Promise<TEntity | null | undefined>;
    List(predicate: any, skip: number, lastId: string, take: number): Promise<Array<TEntity> | null | undefined>;
    Insert(entity: TEntity): Promise<TEntity | string>;
    Update(id: string, entity: TEntity): Promise<TEntity | string | null | undefined>;
    Delete(id: string): Promise<TEntity | string | number | null | undefined>;
}