import redisDriver from 'unstorage/drivers/redis';

export default defineNitroPlugin(() => {
    const storage = useStorage()
    storage.mount('redis', redisDriver({
        base: 'redis',
        host: useRuntimeConfig().redis.host,
        port: useRuntimeConfig().redis.port,
        password: useRuntimeConfig().redis.password,
        db: 0,
        tls: false
    }));
})
