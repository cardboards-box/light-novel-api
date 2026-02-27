import { getData } from '#server/utils/ln-helper';

export default defineEventHandler(async () => {
    const data = await getData();
    return Array.from(new Set(data.map(t => t.publisher))).sort();
});
