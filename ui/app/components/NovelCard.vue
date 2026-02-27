<template>
    <div class="novel-card margin-top margin-left margin-right flex">
        <div
            class="cover-image margin-right center-vert flex"
            :style="{
                maxWidth: '100px',
                minWidth: '100px',
                maxHeight: '150px',
                minHeight: '150px',
                overflow: 'hidden'
            }"
        >
            <Image
                :src="coverUrl"
                clamp-height="150px"
                clamp-width="100px"
                class="rounded center"
            />
        </div>
        <div class="flex row fill novel-content">
            <div class="flex title-container">
                <h3
                    class="title fill center-vert"
                    style="width: calc(100% - 75px);"
                >
                    {{ volume.title }} <span v-if="volume.volume">Vol. {{ volume.volume }}</span>
                </h3>
                <div class="center-vert flex" style="width: 75px;">
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
            <p><b>Series:</b>&nbsp;{{ series.title }}</p>
            <p v-if="volume && volume.volume"><b>Volume #:</b>&nbsp;{{ volume.volume }}</p>
            <p><b>Publisher:</b>&nbsp;{{ publisher.name }}</p>
            <p v-if="publication.isbn"><b>ISBN:</b>&nbsp;{{ publication.isbn }}</p>
            <p v-if="format"><b>Format:</b>&nbsp;{{ format.name }}</p>
            <p>
                <b>Release Date:</b>&nbsp;
                <Date :date="publication.releaseDate" format="D" />
            </p>
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
    padding: var(--margin);
    border: 1px solid var(--color-primary);
    border-radius: var(--brd-radius);
    overflow: hidden;

    &:last-child {
        margin-bottom: var(--margin);
    }

    .novel-content {
        overflow: hidden;

        .title-container {
            min-width: 0;
            .title {
                white-space: nowrap;
                overflow: hidden;
                text-overflow: ellipsis;
            }
        }
    }
}

</style>
