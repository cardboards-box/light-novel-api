import { getCovers } from '#server/utils/ln-helper';

export default defineEventHandler(async (event) => {
    const query = getQuery(event);
    const isbns = Array.isArray(query.isbns) ? query.isbns : [query.isbns].filter(Boolean);
    return await getCovers(isbns);
});
