<template>
    <div class="fill-parent flex row fill scroll-y">
        <Error v-if="error" :message="error" />
        <Loading v-if="pending" />
        <template v-else>

            <div class="flex row">
                <CalendarInput
                    v-model="today"
                    class="margin-top"
                    :range-max="max"
                    :range-min="min"
                    placeholder="Input Calendar"
                />
                <p>Selected: <Date :date="today" /></p>
            </div>

            <NovelCard
                v-for="(novel, index) in novels?.results"
                :key="index"
                :novel="novel"
            />
        </template>
    </div>
</template>

<script setup lang="ts">
import { PublicationFormat } from '~/models';
import type { SearchFilter } from '~/models';
import { DateTime } from 'luxon';

const dates = useDateUtils();

const today = ref(new Date());
const min = ref(DateTime.now().minus({ years: 1 }).toJSDate());
const max = ref(DateTime.now().plus({ years: 1 }).toJSDate());

const filters = ref<SearchFilter>({
    start: dates.startOf.month(new Date()).toJSDate(),
    end: dates.endOf.month(new Date()).toJSDate(),
    publishers: ['J-Novel Club', 'Yen Press'],
    asc: true,
    refresh: false,
    formats: [PublicationFormat.Digital]
});
const { pending: novPending, error: novError, data: novels, refresh } = useNovelSearch('index', filters);
const { pending: pubPending, error: pubError, data: publishers } = usePublishers();

const pending = computed(() => novPending.value || pubPending.value);
const error = computed(() => novError.value?.toString() || pubError.value?.toString());

</script>

<style lang="scss" scoped>

</style>
