/* JS for Blogs Search Page */
document.addEventListener('DOMContentLoaded', function() {
    const filterToggleBtn = document.getElementById('filterToggleBtn');
    const filterPanel = document.getElementById('advancedFilterPanel');
    const sortWrapper = document.getElementById('blogSortWrapper');
    const sortTrigger = document.getElementById('blogSortTrigger');
    const sortOptions = document.querySelectorAll('.sort-option-item');
    const advancedFilterForm = document.getElementById('advancedFilterForm');
    const currentSortBy = document.getElementById('currentSortBy');

    // Reusable logic for multi-dropdowns
    const dropdowns = document.querySelectorAll('.modern-dropdown-wrapper-premium');
    
    dropdowns.forEach(wrapper => {
        const trigger = wrapper.querySelector('.dropdown-trigger-premium');
        const items = wrapper.querySelectorAll('.dropdown-item-premium');
        const input = wrapper.querySelector('input[type="hidden"]');
        const label = wrapper.querySelector('.selected-label');

        if (trigger) {
            trigger.addEventListener('click', function(e) {
                e.stopPropagation();
                // Close other dropdowns
                dropdowns.forEach(other => {
                    if (other !== wrapper) other.classList.remove('active');
                });
                if (sortWrapper) sortWrapper.classList.remove('active');
                wrapper.classList.toggle('active');
            });
        }

        items.forEach(item => {
            item.addEventListener('click', function() {
                const val = this.getAttribute('data-value');
                const text = this.querySelector('span').innerText;
                
                input.value = val;
                label.innerText = text;
                
                // Update active state
                items.forEach(i => i.classList.remove('active'));
                this.classList.add('active');
                
                wrapper.classList.remove('active');
            });
        });
    });

    // Toggle Filter Panel
    if (filterToggleBtn && filterPanel) {
        filterToggleBtn.addEventListener('click', function() {
            const isOpen = filterPanel.style.display === 'block';
            filterPanel.style.display = isOpen ? 'none' : 'block';
            filterToggleBtn.classList.toggle('active');
        });
    }

    // Sort Dropdown Toggle
    if (sortTrigger && sortWrapper) {
        sortTrigger.addEventListener('click', function(e) {
            e.stopPropagation();
            // Close filter dropdowns
            dropdowns.forEach(d => d.classList.remove('active'));
            sortWrapper.classList.toggle('active');
        });

        document.addEventListener('click', function() {
            sortWrapper.classList.remove('active');
            dropdowns.forEach(d => d.classList.remove('active'));
        });

        sortOptions.forEach(option => {
            option.addEventListener('click', function() {
                if (currentSortBy && advancedFilterForm) {
                    currentSortBy.value = this.getAttribute('data-value');
                    advancedFilterForm.submit();
                }
            });
        });
    }

    // Initialize AOS if available
    if (typeof AOS !== 'undefined') {
        AOS.init({ duration: 1000, once: true, offset: 100 });
    }
});
