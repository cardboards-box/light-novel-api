<template>
    <div class="novel-card margin-top margin-left margin-right flex">
        <Image
            v-if="novel.cover"
            :src="novel.cover"
            clamp-height="150px"
            class="rounded margin-right"
        />
        <div class="flex row fill">
            <div class="flex">
                <h3 class="fill center-vert">{{ novel.title }} <span v-if="novel.volume">Vol. {{ novel.volume }}</span></h3>
                <div class="center-vert flex">
                    <Icon v-if="novel.isPhysical">book_2</Icon>
                    <Icon v-if="novel.isDigital">tablet_android</Icon>
                    <Icon v-if="novel.isAudioBook">headphones</Icon>
                    <IconBtn
                        :link="novel.url"
                        icon="open_in_new"
                        title="Open Novel Link"
                        inline
                        color="primary"
                        external
                    />
                    <IconBtn
                        v-if="novel.amazonLink"
                        :link="novel.amazonLink"
                        icon="shopping_cart_checkout"
                        title="Buy on Amazon"
                        inline
                        color="primary"
                        external
                    />
                </div>
            </div>
            <p><b>Series:</b>&nbsp;{{ novel.series }}</p>
            <p v-if="novel.volume"><b>Volume #:</b>&nbsp;{{ novel.volume }}</p>
            <p><b>Publisher:</b>&nbsp;{{ novel.publisher }}</p>
            <p v-if="novel.isbn"><b>ISBN:</b>&nbsp;{{ novel.isbn }}</p>
            <p v-if="formats.length > 0"><b>Format{{ formats.length > 1 ? 's' : '' }}:</b>&nbsp;{{ formats.map(f => f.name).join(', ') }}</p>
            <p>
                <b>Release Date:</b>&nbsp;
                <Date :date="novel.date" format="D" />
            </p>
        </div>
    </div>
</template>

<script setup lang="ts">
import type { Novel } from '~/models';

const props = defineProps<{
    novel: Novel;
}>();

const formats = computed(() => {
    const formats = [];

    if (props.novel.isPhysical) formats.push({
        name: 'Physical',
        icon: 'book_2'
    });

    if (props.novel.isDigital) formats.push({
        name: 'Digital',
        icon: 'tablet_android'
    });

    if (props.novel.isAudioBook) formats.push({
        name: 'Audio Book',
        icon: 'headphones'
    });

    return formats;
})

</script>

<style lang="scss" scoped>
.novel-card {
    padding: var(--margin);
    border: 1px solid var(--color-primary);
    border-radius: var(--brd-radius);

    &:last-child {
        margin-bottom: var(--margin);
    }
}

</style>
