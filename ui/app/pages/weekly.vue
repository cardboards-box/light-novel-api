<template>
    <div class="fill fill-parent flex row scroll-y">
        <div class="center-horz margin-top max-width flex">
            <InputGroup
                v-model="searchFilters.search"
                placeholder="Search for your favourite novel!"
                @search="() => doSearch()"
                is-drawer
            >
                <template #input>
                    <div class="flex">
                        <IconBtn
                            class="center-vert"
                            icon="arrow_back"
                            @click="move(-1)"
                            inline
                        />
                        <p class="center-vert">
                            <Date :date="date" format="d" />
                        </p>
                        <IconBtn
                            class="center-vert"
                            icon="today"
                            @click="today"
                            inline
                        />
                        <IconBtn
                            class="center-vert"
                            icon="arrow_forward"
                            @click="move(1)"
                            inline
                        />
                    </div>
                </template>

                <h3 class="margin-bottom">Advanced Search Options:</h3>

                <label class="margin-top">Date:</label>
                <CalendarInput
                    placeholder="Week Date"
                    v-model="date"
                />

                <label>Formats:</label>
                <ButtonGroup :options="formats" v-model="searchFilters.format" />

                <label>Publishers:</label>
                <ButtonGroup :options="publishers" v-model="searchFilters.publisher" />

                <label class="margin-top">Order Direction:</label>
                <ButtonGroupBool v-model="searchFilters.asc" on="Ascending" off="Descending" />

                <div class="flex align-left">
                    <button class="icon-btn" @click="clear()">
                        <Icon>delete</Icon>
                        <p>Clear</p>
                    </button>
                    <button class="icon-btn" @click="doSearch()">
                        <Icon>search</Icon>
                        <p>Search</p>
                    </button>
                </div>
            </InputGroup>
        </div>

        <Error v-if="error" :message="error" />
        <Loading v-else-if="pending" />
        <div v-else class="calendar margin-top fill">
            <div
                class="day flex row"
                v-for="(day, index) in calendar?.entries[0]?.entries ?? []"
                :key="index"
            >
                <header class="center-horz">
                    <b><Date :date="day.date" parse-format="yyyy-MM-dd" format="ccc" /></b>
                    &nbsp;
                    <Date :date="day.date" parse-format="yyyy-MM-dd" format="LLL d" />
                </header>
                <NovelScheduleItem
                    v-for="(novel, index) in day.entities ?? []"
                    :key="index"
                    :novel="calendar!.entities[novel]!"
                />
                <p class="center-horz margin-top" v-if="!day.entities?.length">No Releases</p>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import type { SearchFilterBase } from '~/models';

type SearchFilter = SearchFilterBase & {
    date?: Date | string;
};

const route = useRoute();
const dateUtils = useDateUtils();
const { parse, setup, update, clear } = useCalendarHelper(true);

const _error = ref<string>();
const filter = computed(() => parse());
const searchFilters = ref(parse());

const { publishers, formats, calendar, pending, error: apiError, refresh } = setup();

const date = computed({
    get: () => searchFilters.value.date ?? new Date(),
    set: (value: Date | string) => searchFilters.value.date = value
});

const error = computed({
    get: () => _error.value || apiError.value?.toString(),
    set: (value) => _error.value = value
});

function doSearch() {
    updateRoute({ ...searchFilters.value });
}

function updateRoute(merge?: Partial<SearchFilter>) {
    update(filter.value, merge);
}


function move(step: number) {
    let current = dateUtils.ensureDate(date.value);
    current = current.plus({ days: step * 7 });
    current = dateUtils.middleOf.week(current);
    date.value = current.toJSDate();
    doSearch();
}

function today() {
    date.value = new Date();
    doSearch();
}

watch(() => route.query, () => refresh(), { deep: true });
</script>

<style lang="scss" scoped>
.calendar {
    max-width: 100%;
    overflow-x: auto;
    padding: var(--margin);
    display: grid;
    //grid-template-columns: repeat(auto-fill, minmax(330px, 1fr));
    grid-template-columns: repeat(7, minmax(330px, 1fr));
    gap: var(--margin);

    .day {
        min-width: 330px;
    }
}
</style>
