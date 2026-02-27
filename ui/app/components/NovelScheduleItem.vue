<template>
    <div class="novel-card flex margin-top">
        <div
            class="cover-image margin-right center-vert flex"
            :style="{
                maxWidth: '75px',
                minWidth: '75px',
                maxHeight: '100px',
                minHeight: '100px',
                overflow: 'hidden'
            }"
        >
            <Image
                :src="coverUrl"
                clamp-height="100px"
                clamp-width="75px"
                class="rounded center"
            />
        </div>
        <div class="flex row fill">
            <p><b>{{ volume.title || series.title }}</b></p>
            <p v-if="volume.volume">Vol. {{ volume.volume }}</p>
            <div class="flex">
                <Icon v-if="format" :title="format.name">{{ format.icon }}</Icon>
                <IconBtn
                    v-if="publication.url"
                    :link="publication.url"
                    icon="open_in_new"
                    title="Open Novel Link"
                    inline
                    color="primary"
                    external
                />
                <IconBtn
                    v-if="amazon"
                    :link="amazon"
                    icon="shopping_cart_checkout"
                    title="Buy on Amazon"
                    inline
                    color="primary"
                    external
                />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { LncFormat } from '~/models';
import type { LncFullPublication } from '~/models';
const { getRelated, amazonLink, promise } = useApi();

const props = defineProps<{
    novel: LncFullPublication;
}>();

const format = computed(() => {
    switch (props.novel.entity.format) {
        case LncFormat.Physical: return { name: 'Physical', icon: 'book_2' };
        case LncFormat.Digital: return { name: 'Digital', icon: 'tablet_android' };
        case LncFormat.Audio: return { name: 'Audio Book', icon: 'headphones' };
    }
});

const cover = computed(() => getRelated(props.novel, 'LncCover'));
const coverUrl = computed(() => {
    if (cover.value && cover.value.coverUrl)
        return promise.image.url(cover.value.id);
    if (publisher.value && publisher.value.iconUrl)
        return publisher.value.iconUrl;
    return '/error.gif';
});
const volume = computed(() => getRelated(props.novel, 'LncVolume')!);
const publisher = computed(() => getRelated(props.novel, 'LncPublisher')!);
const series = computed(() => getRelated(props.novel, 'LncSeries')!);
const publication = computed(() => props.novel.entity);
const amazon = computed(() => amazonLink(props.novel));
</script>

<style lang="scss" scoped>
.novel-card {
    background-color: var(--bg-color-accent);
    padding: var(--margin);
    border-radius: calc(var(--brd-radius) * 2);
}
</style>
