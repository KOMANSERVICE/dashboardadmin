window.DragDropInterop = {
    initSortable: function (tableId, dotNetHelper) {
        const table = document.getElementById(tableId);
        if (!table) return;

        const tbody = table.querySelector('tbody');
        if (!tbody) return;

        let draggedRow = null;
        let placeholder = null;

        const rows = tbody.querySelectorAll('tr[data-reference]');
        rows.forEach(row => {
            row.setAttribute('draggable', 'true');

            row.addEventListener('dragstart', function (e) {
                draggedRow = this;
                this.classList.add('dragging');
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData('text/plain', this.dataset.reference);

                placeholder = document.createElement('tr');
                placeholder.className = 'drop-placeholder';
                placeholder.innerHTML = '<td colspan="' + this.cells.length + '"></td>';
            });

            row.addEventListener('dragend', function () {
                this.classList.remove('dragging');
                if (placeholder && placeholder.parentNode) {
                    placeholder.parentNode.removeChild(placeholder);
                }
                draggedRow = null;
                placeholder = null;

                const newOrder = [];
                tbody.querySelectorAll('tr[data-reference]').forEach((row, index) => {
                    newOrder.push({
                        reference: row.dataset.reference,
                        sortOrder: index
                    });
                });

                dotNetHelper.invokeMethodAsync('OnOrderChanged', newOrder);
            });

            row.addEventListener('dragover', function (e) {
                e.preventDefault();
                e.dataTransfer.dropEffect = 'move';

                if (this === draggedRow) return;

                const rect = this.getBoundingClientRect();
                const midY = rect.top + rect.height / 2;

                if (e.clientY < midY) {
                    this.parentNode.insertBefore(placeholder, this);
                } else {
                    this.parentNode.insertBefore(placeholder, this.nextSibling);
                }
            });

            row.addEventListener('drop', function (e) {
                e.preventDefault();
                if (draggedRow && placeholder && placeholder.parentNode) {
                    placeholder.parentNode.insertBefore(draggedRow, placeholder);
                }
            });
        });
    },

    initMobileSortable: function (containerId, dotNetHelper) {
        const container = document.getElementById(containerId);
        if (!container) return;

        let draggedCard = null;
        let placeholder = null;
        let touchStartY = 0;
        let initialTop = 0;

        const cards = container.querySelectorAll('[data-reference]');
        cards.forEach(card => {
            const handle = card.querySelector('.drag-handle');
            if (!handle) return;

            handle.addEventListener('touchstart', function (e) {
                draggedCard = card;
                touchStartY = e.touches[0].clientY;
                initialTop = card.offsetTop;
                card.classList.add('dragging');

                placeholder = document.createElement('div');
                placeholder.className = 'drop-placeholder-mobile';
                placeholder.style.height = card.offsetHeight + 'px';
            }, { passive: true });

            handle.addEventListener('touchmove', function (e) {
                if (!draggedCard) return;
                e.preventDefault();

                const touchY = e.touches[0].clientY;
                const deltaY = touchY - touchStartY;

                draggedCard.style.transform = 'translateY(' + deltaY + 'px)';
                draggedCard.style.zIndex = '1000';

                const allCards = Array.from(container.querySelectorAll('[data-reference]:not(.dragging)'));
                for (const targetCard of allCards) {
                    const rect = targetCard.getBoundingClientRect();
                    const midY = rect.top + rect.height / 2;

                    if (touchY < midY && touchY > rect.top) {
                        container.insertBefore(placeholder, targetCard);
                        break;
                    } else if (touchY > midY && touchY < rect.bottom) {
                        container.insertBefore(placeholder, targetCard.nextSibling);
                        break;
                    }
                }
            }, { passive: false });

            handle.addEventListener('touchend', function () {
                if (!draggedCard) return;

                draggedCard.style.transform = '';
                draggedCard.style.zIndex = '';
                draggedCard.classList.remove('dragging');

                if (placeholder && placeholder.parentNode) {
                    placeholder.parentNode.insertBefore(draggedCard, placeholder);
                    placeholder.parentNode.removeChild(placeholder);
                }

                const newOrder = [];
                container.querySelectorAll('[data-reference]').forEach((card, index) => {
                    newOrder.push({
                        reference: card.dataset.reference,
                        sortOrder: index
                    });
                });

                dotNetHelper.invokeMethodAsync('OnOrderChanged', newOrder);

                draggedCard = null;
                placeholder = null;
            });
        });
    },

    dispose: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            const clonedElement = element.cloneNode(true);
            element.parentNode.replaceChild(clonedElement, element);
        }
    }
};
