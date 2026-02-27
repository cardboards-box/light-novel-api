import { search } from '#server/utils/ln-helper';
import { SearchFilter, SearchFilterServer } from '~/models';

export default defineEventHandler(async (event) => {
    const server = getQuery<SearchFilterServer>(event);
    const query: SearchFilter = {};
    if (server.start && typeof server.start === 'string') query.start = new Date(server.start);
    if (server.end && typeof server.end === 'string') query.end = new Date(server.end);
    if (server.publishers) {
        if (typeof server.publishers === 'string') query.publishers = [server.publishers];
        else query.publishers = server.publishers;
    }
    if (server.released !== undefined) query.released = server.released === 'true';
    if (server.formats) {
        if (typeof server.formats === 'string') query.formats = [+server.formats];
        else query.formats = server.formats.map(f => +f);
    }
    if (server.search && typeof server.search === 'string') query.search = server.search;
    if (server.asc !== undefined) query.asc = server.asc === 'true';
    if (server.refresh !== undefined) query.refresh = server.refresh === 'true';
    query.size = +(server.size ?? '20');
    if (query.size > 100) query.size = 100;
    if (query.size < 1) query.size = 1;
    query.page = +(server.page ?? '1');
    if (query.page < 1) query.page = 1;
    return await search(query);
});
