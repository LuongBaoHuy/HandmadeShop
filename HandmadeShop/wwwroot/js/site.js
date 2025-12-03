// Hiển thị số lượng sản phẩm trong giỏ hàng (nếu cần cập nhật động)
document.addEventListener('DOMContentLoaded', function () {
    // Ví dụ: cập nhật cart-count từ localStorage (nếu bạn dùng localStorage)
    // let cartCount = localStorage.getItem('cartCount');
    // if (cartCount) {
    //     document.getElementById('cart-count').textContent = cartCount;
    // }

    // Đóng navbar khi chọn menu trên mobile
    var navLinks = document.querySelectorAll('.navbar-nav .nav-link');
    var navbarCollapse = document.getElementById('navbarNav');
    if (navLinks && navbarCollapse) {
        navLinks.forEach(function (link) {
            link.addEventListener('click', function () {
                if (window.innerWidth < 992 && navbarCollapse.classList.contains('show')) {
                    var bsCollapse = new bootstrap.Collapse(navbarCollapse, { toggle: false });
                    bsCollapse.hide();
                }
            });
        });
    }

    // Hiển thị dropdown khi hover vào giỏ hàng
    const cartDropdown = document.querySelector('.header-cart .dropdown-menu');
    const cartToggle = document.querySelector('.header-cart .dropdown-toggle');

    if (cartToggle && cartDropdown) {
        cartToggle.addEventListener('mouseenter', function () {
            cartDropdown.classList.add('show');
        });

        cartToggle.addEventListener('mouseleave', function () {
            cartDropdown.classList.remove('show');
        });

        cartDropdown.addEventListener('mouseenter', function () {
            cartDropdown.classList.add('show');
        });

        cartDropdown.addEventListener('mouseleave', function () {
            cartDropdown.classList.remove('show');
        });
    }

    // Hiển thị dropdown khi hover vào user
    const userAvatar = document.querySelector('.header-user .dropdown-toggle');
    const userDropdown = document.querySelector('.header-user .user-dropdown');

    if (userAvatar && userDropdown) {
        userAvatar.addEventListener('mouseenter', function () {
            userDropdown.style.display = 'block';
        });

        userAvatar.addEventListener('mouseleave', function () {
            userDropdown.style.display = 'none';
        });

        userDropdown.addEventListener('mouseenter', function () {
            this.style.display = 'block';
        });

        userDropdown.addEventListener('mouseleave', function () {
            this.style.display = 'none';
        });
    }

    // Khởi tạo Bootstrap Carousel cho hero banner
    const mainBannerCarousel = document.getElementById('mainBanner');
    if (mainBannerCarousel) {
        console.log('Initializing main banner carousel...');
        
        // Lấy hoặc tạo Bootstrap Carousel instance
        const carousel = bootstrap.Carousel.getOrCreateInstance(mainBannerCarousel, {
            interval: 5000,
            touch: true,
            ride: true,
            wrap: true,
            keyboard: true,
            pause: 'hover'
        });

        // Bắt đầu auto-slide
        carousel.cycle();
        console.log('Carousel cycle started');

        // Reset và trigger animation khi slide thay đổi
        mainBannerCarousel.addEventListener('slide.bs.carousel', function(e) {
            console.log('Slide changing...');
            // Reset animation cho slide hiện tại
            const currentSlide = e.target.querySelector('.carousel-item.active');
            if (currentSlide) {
                const animatedElements = currentSlide.querySelectorAll('.fadeInDown-1, .fadeInDown-2, .fadeInDown-3');
                animatedElements.forEach(el => {
                    el.style.animation = 'none';
                });
            }
        });

        mainBannerCarousel.addEventListener('slid.bs.carousel', function(e) {
            console.log('Slide changed');
            // Trigger animation cho slide mới
            const newSlide = e.target.querySelector('.carousel-item.active');
            if (newSlide) {
                const animatedElements = newSlide.querySelectorAll('.fadeInDown-1, .fadeInDown-2, .fadeInDown-3');
                animatedElements.forEach(el => {
                    // Force reflow để restart animation
                    el.offsetHeight;
                    el.style.animation = '';
                });
            }
        });
    } else {
        console.log('Main banner carousel element not found');
    }

    // Khởi tạo Owl Carousel cho banner chính
    if (typeof $.fn.owlCarousel === 'function') {
        // Banner chính
        if ($('#owl-main').length) {
            $('#owl-main').owlCarousel({
                loop: true,
                margin: 0,
                nav: true,
                navText: ['<i class="fa fa-chevron-left"></i>', '<i class="fa fa-chevron-right"></i>'],
                dots: true,
                autoplay: true,
                autoplayTimeout: 5000,
                autoplayHoverPause: true,
                animateOut: 'fadeOut',
                animateIn: 'fadeIn',
                smartSpeed: 1000,
                responsive: {
                    0: { items: 1 },
                    600: { items: 1 },
                    1000: { items: 1 }
                },
                onInitialized: function() {
                    $('.owl-item.active .item').addClass('active');
                },
                onTranslate: function() {
                    $('.owl-item .item').removeClass('active');
                    $('.owl-item.active .item').addClass('active');
                }
            });
        }

        // Sản phẩm mới
        if ($('#new-products-carousel').length) {
            $('#new-products-carousel').owlCarousel({
                loop: true,
                margin: 15,
                nav: true,
                navText: ['<i class="fa fa-chevron-left"></i>', '<i class="fa fa-chevron-right"></i>'],
                dots: false,
                responsive: {
                    0: { items: 1 },
                    600: { items: 2 },
                    1000: { items: 4 }
                }
            });
        }

        // Featured Products Carousel
        if ($('.featured-product .owl-carousel').length) {
            $('.featured-product .owl-carousel').owlCarousel({
                loop: true,
                margin: 20,
                nav: true,
                navText: ['<i class="fa fa-chevron-left"></i>', '<i class="fa fa-chevron-right"></i>'],
                dots: true,
                autoplay: true,
                autoplayTimeout: 6000,
                autoplayHoverPause: true,
                smartSpeed: 800,
                responsive: {
                    0: { 
                        items: 1,
                        margin: 10
                    },
                    576: { 
                        items: 2,
                        margin: 15
                    },
                    768: { 
                        items: 2,
                        margin: 20
                    },
                    992: { 
                        items: 3,
                        margin: 20
                    },
                    1200: { 
                        items: 4,
                        margin: 20
                    }
                },
                onInitialized: function() {
                    $('.featured-product .owl-item').addClass('animate-slide-in');
                },
                onTranslated: function() {
                    $('.featured-product .owl-item.active').addClass('animate-slide-in');
                }
            });
        }

        // New Arrivals Carousel (Main carousel for new products)
        if ($('.new-arriavls .owl-carousel').length) {
            $('.new-arriavls .owl-carousel').owlCarousel({
                loop: true,
                margin: 20,
                nav: true,
                navText: ['<i class="fa fa-chevron-left"></i>', '<i class="fa fa-chevron-right"></i>'],
                dots: true,
                autoplay: true,
                autoplayTimeout: 5000,
                autoplayHoverPause: true,
                smartSpeed: 800,
                responsive: {
                    0: { 
                        items: 1,
                        margin: 10
                    },
                    576: { 
                        items: 2,
                        margin: 15
                    },
                    768: { 
                        items: 2,
                        margin: 20
                    },
                    992: { 
                        items: 3,
                        margin: 20
                    },
                    1200: { 
                        items: 4,
                        margin: 20
                    }
                },
                onInitialized: function() {
                    // Add fade-in animation when carousel is initialized
                    $('.new-arriavls .owl-item').addClass('animate-fade-in');
                },
                onTranslated: function() {
                    // Re-animate items after slide transition
                    $('.new-arriavls .owl-item.active').addClass('animate-fade-in');
                }
            });
        }

        // Hot deals
        if ($('.sidebar-carousel').length) {
            $('.sidebar-carousel').owlCarousel({
                items: 1,
                loop: true,
                margin: 10,
                nav: true,
                navText: ['<i class="fa fa-chevron-left"></i>', '<i class="fa fa-chevron-right"></i>'],
                dots: false,
                navContainer: '.hot-deals .custom-nav',
                responsive: {
                    0: { items: 1 },
                    600: { items: 1 },
                    1000: { items: 1 }
                }
            });
        }
    }

    // Xử lý nút thêm vào giỏ hàng
    const addToCartButtons = document.querySelectorAll('.add-to-cart-btn, .cart-btn');
    addToCartButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            
            // Lấy thông tin sản phẩm từ card
            const productCard = this.closest('.products, .product');
            if (productCard) {
                const productName = productCard.querySelector('.name a')?.textContent?.trim();
                const productPrice = productCard.querySelector('.price')?.textContent?.trim();
                const productImage = productCard.querySelector('.product-image img')?.src;
                const productLink = productCard.querySelector('.name a')?.href;
                
                // Tạo object sản phẩm
                const product = {
                    id: Date.now(), // Tạm thời dùng timestamp làm ID
                    name: productName,
                    price: productPrice,
                    image: productImage,
                    link: productLink,
                    quantity: 1
                };
                
                // Thêm vào giỏ hàng
                addToCart(product);
                
                // Hiệu ứng cho nút
                this.style.transform = 'scale(0.95)';
                setTimeout(() => {
                    this.style.transform = 'scale(1)';
                }, 150);
                
                // Hiển thị thông báo
                showToast(`Đã thêm "${productName}" vào giỏ hàng!`);
            }
        });
    });

    // Hàm thêm sản phẩm vào giỏ hàng
    function addToCart(product) {
        let cart = JSON.parse(localStorage.getItem('cart')) || [];
        const existingItem = cart.find(item => item.id === product.id);
        
        if (existingItem) {
            existingItem.quantity += 1;
        } else {
            cart.push(product);
        }
        
        localStorage.setItem('cart', JSON.stringify(cart));
        updateCartCount();
    }

    // Cập nhật số lượng sản phẩm trong giỏ hàng
    function updateCartCount() {
        const cart = JSON.parse(localStorage.getItem('cart')) || [];
        const totalItems = cart.reduce((total, item) => total + (item.quantity || 1), 0);
        const cartCountElements = document.querySelectorAll('.cart-count');
        
        cartCountElements.forEach(element => {
            element.textContent = totalItems;
        });
    }

    // Hiển thị thông báo
    function showToast(message) {
        const toast = document.createElement('div');
        toast.className = 'toast-notification';
        toast.textContent = message;
        document.body.appendChild(toast);
        
        // Thêm class để hiệu ứng hiển thị
        setTimeout(() => {
            toast.classList.add('show');
        }, 100);
        
        // Tự động ẩn sau 3 giây
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => {
                document.body.removeChild(toast);
            }, 300);
        }, 3000);
    }
    
    // Cập nhật số lượng giỏ hàng khi tải trang
    updateCartCount();

    // Featured Products và Best Seller Add to Cart functionality
    $(document).on('click', '.featured-product .add-cart-button .cart-btn, .best-deal .add-cart-button .cart-btn', function(e) {
        e.preventDefault();
        
        const productCard = $(this).closest('.products, .best-product');
        const productId = productCard.find('[data-product-id]').data('product-id') || Math.random().toString(36).substr(2, 9);
        const productName = productCard.find('.name a').text().trim();
        const productPrice = productCard.find('.price').text().trim().replace(/[^\d,]/g, '');
        const productImage = productCard.find('.product-image img').attr('src') || '/images/default-product.jpg';
        
        // Animation hiệu ứng khi thêm vào giỏ
        const cartIcon = $(this).find('i');
        cartIcon.removeClass('fa-shopping-cart').addClass('fa-check');
        $(this).addClass('btn-success').removeClass('btn-primary');
        
        // Hiệu ứng bay lên giỏ hàng
        const flyEffect = $('<div class="cart-fly-effect"><i class="fa fa-shopping-cart"></i></div>');
        flyEffect.css({
            position: 'fixed',
            left: $(this).offset().left,
            top: $(this).offset().top,
            'z-index': 9999,
            color: '#e74c3c',
            'font-size': '20px',
            'pointer-events': 'none'
        });
        
        $('body').append(flyEffect);
        
        flyEffect.animate({
            top: '20px',
            right: '20px',
            opacity: 0
        }, 1000, function() {
            flyEffect.remove();
        });
        
        // Lưu vào localStorage (giả lập)
        let cart = JSON.parse(localStorage.getItem('cart') || '[]');
        const existingItem = cart.find(item => item.id === productId);
        
        if (existingItem) {
            existingItem.quantity += 1;
        } else {
            cart.push({
                id: productId,
                name: productName,
                price: productPrice,
                image: productImage,
                quantity: 1
            });
        }
        
        localStorage.setItem('cart', JSON.stringify(cart));
        updateCartCount();
        
        // Hiển thị thông báo
        showCartNotification(productName);
        
        // Reset nút về trạng thái ban đầu sau 2 giây
        setTimeout(() => {
            cartIcon.removeClass('fa-check').addClass('fa-shopping-cart');
            $(this).removeClass('btn-success').addClass('btn-primary');
        }, 2000);
    });

    // Hiển thị thông báo khi thêm sản phẩm
    function showCartNotification(productName) {
        const notification = $(`
            <div class="cart-notification">
                <i class="fa fa-check-circle"></i>
                <span>Đã thêm "${productName}" vào giỏ hàng!</span>
            </div>
        `);
        
        notification.css({
            position: 'fixed',
            top: '20px',
            right: '20px',
            background: '#27ae60',
            color: 'white',
            padding: '15px 20px',
            'border-radius': '5px',
            'z-index': 10000,
            'box-shadow': '0 4px 15px rgba(0,0,0,0.2)',
            transform: 'translateX(100%)',
            transition: 'transform 0.3s ease'
        });
        
        $('body').append(notification);
        
        setTimeout(() => {
            notification.css('transform', 'translateX(0)');
        }, 100);
        
        setTimeout(() => {
            notification.css('transform', 'translateX(100%)');
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }

    // Thêm hiệu ứng hover cho product cards trong tất cả sections
    const allProductCards = document.querySelectorAll('.products, .best-product');
    allProductCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            if (!this.style.transform || this.style.transform === 'none') {
                this.style.transform = 'translateY(-5px)';
                this.style.boxShadow = '0 8px 30px rgba(0,0,0,0.15)';
            }
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
            this.style.boxShadow = '0 4px 20px rgba(0,0,0,0.08)';
        });
    });

    // Lazy loading cho hình ảnh trong tất cả product cards
    const allProductImages = document.querySelectorAll('.product-image img');
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                if (img.dataset.src) {
                    img.src = img.dataset.src;
                    img.classList.add('loaded');
                    observer.unobserve(img);
                }
            }
        });
    });

    allProductImages.forEach(img => {
        imageObserver.observe(img);
    });

    // Animate elements khi scroll vào view
    const animateOnScroll = () => {
        const elements = document.querySelectorAll('.new-arriavls, .new-products-section');
        elements.forEach(element => {
            const elementTop = element.getBoundingClientRect().top;
            const elementVisible = 150;
            
            if (elementTop < window.innerHeight - elementVisible) {
                element.classList.add('animate-fade-in');
            }
        });
    };

    window.addEventListener('scroll', animateOnScroll);
    animateOnScroll(); // Run once on load

    // Khởi tạo Hot Deals Carousel
    document.addEventListener('DOMContentLoaded', function() {
        // Hot Deals Carousel
        const hotDealsCarousel = document.getElementById('hotDealsCarousel');
        if (hotDealsCarousel) {
            new bootstrap.Carousel(hotDealsCarousel, {
                interval: 5000,
                touch: true
            });
        }

        // Special Offer Carousel
        const specialOfferCarousel = document.getElementById('specialOfferCarousel');
        if (specialOfferCarousel) {
            new bootstrap.Carousel(specialOfferCarousel, {
                interval: 5000,
                touch: true
            });
        }
    });

    // Xử lý sự kiện cho nút thu gọn/mở rộng danh mục
    const toggleCategoriesBtn = document.querySelector('.toggle-categories');
    const categoryListContainer = document.querySelector('.category-list-container');
    
    if (toggleCategoriesBtn && categoryListContainer) {
        // Kiểm tra xem danh sách có đang bị ẩn không (mặc định là mở)
        const isCollapsed = localStorage.getItem('categoryMenuCollapsed') === 'true';
        
        // Áp dụng trạng thái từ localStorage
        if (isCollapsed) {
            categoryListContainer.classList.add('collapsed');
            toggleCategoriesBtn.classList.add('collapsed');
        }
        
        // Xử lý sự kiện click
        toggleCategoriesBtn.addEventListener('click', function() {
            categoryListContainer.classList.toggle('collapsed');
            toggleCategoriesBtn.classList.toggle('collapsed');
            
            // Lưu trạng thái vào localStorage
            const isNowCollapsed = categoryListContainer.classList.contains('collapsed');
            localStorage.setItem('categoryMenuCollapsed', isNowCollapsed);
        });
    }

    userAvatar.addEventListener('mouseleave', function () {
        userDropdown.style.display = 'none';
    });

    userDropdown.addEventListener('mouseenter', function () {
        userDropdown.style.display = 'block';
    });

    userDropdown.addEventListener('mouseleave', function () {
        userDropdown.style.display = 'none';
    });
});

// Loại bỏ phần khởi tạo Owl Carousel cho Featured Products
// và thay thế bằng grid animation
document.addEventListener('DOMContentLoaded', function () {
    // Animate featured products grid items on scroll
    const featuredProductItems = document.querySelectorAll('.featured-product-item');
    
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.animationPlayState = 'running';
            }
        });
    }, observerOptions);
    
    featuredProductItems.forEach(item => {
        item.style.animationPlayState = 'paused';
        observer.observe(item);
    });
});

// ==============================================
// CATEGORY FILTER FUNCTIONALITY (Updated)
// ==============================================

document.addEventListener('DOMContentLoaded', function() {
    // Category page specific functionality  
    if (document.querySelector('.category-product')) {
        initializeCategoryFilters();
    }
});

function initializeCategoryFilters() {
    // Initialize price filter
    initializePriceFilter();
    
    // Initialize dropdown functionality
    initializeDropdowns();
    
    // Initialize wishlist functionality
    initializeWishlist();
    
    // Initialize rating animations
    initializeRatingAnimations();
}

// Rating Animations
function initializeRatingAnimations() {
    const ratingStars = document.querySelectorAll('.rating-stars');
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const stars = entry.target.querySelectorAll('.star-filled');
                stars.forEach((star, index) => {
                    setTimeout(() => {
                        star.style.animation = 'starPop 0.3s ease forwards';
                    }, index * 100);
                });
            }
        });
    });
    
    ratingStars.forEach(rating => {
        observer.observe(rating);
    });
}

// Star pop animation keyframes
const starPopKeyframes = `
@keyframes starPop {
    0% { transform: scale(0.5); opacity: 0; }
    50% { transform: scale(1.2); }
    100% { transform: scale(1); opacity: 1; }
}`;

// Add keyframes to document
const style = document.createElement('style');
style.textContent = starPopKeyframes;
document.head.appendChild(style);

// Price Filter Functionality
function initializePriceFilter() {
    const priceInputs = document.querySelectorAll('.price-input');
    const priceQuickBtns = document.querySelectorAll('.price-quick-btn');
    const priceForm = document.getElementById('priceFilterForm');
    
    // Price input change handler
    priceInputs.forEach(input => {
        input.addEventListener('input', function() {
            // Remove active state from quick select buttons
            priceQuickBtns.forEach(btn => {
                btn.classList.remove('btn-primary');
                btn.classList.add('btn-outline-primary');
            });
        });
    });
    
    // Quick price select buttons
    priceQuickBtns.forEach(button => {
        button.addEventListener('click', function() {
            const minPrice = this.getAttribute('data-min');
            const maxPrice = this.getAttribute('data-max');
            
            document.querySelector('input[name="minPrice"]').value = minPrice;
            document.querySelector('input[name="maxPrice"]').value = 
                maxPrice === '999999999' ? '' : maxPrice;
            
            // Update button states
            priceQuickBtns.forEach(btn => {
                btn.classList.remove('btn-primary');
                btn.classList.add('btn-outline-primary');
            });
            this.classList.remove('btn-outline-primary');
            this.classList.add('btn-primary');
            
            // Auto-submit form
            setTimeout(() => {
                priceForm.submit();
            }, 300);
        });
    });
    
    // Auto-submit on price input change (with debounce)
    let priceTimeout;
    priceInputs.forEach(input => {
        input.addEventListener('change', function() {
            clearTimeout(priceTimeout);
            priceTimeout = setTimeout(() => {
                const minPrice = document.querySelector('input[name="minPrice"]').value;
                const maxPrice = document.querySelector('input[name="maxPrice"]').value;
                
                if (minPrice || maxPrice) {
                    priceForm.submit();
                }
            }, 800);
        });
    });
}

// Dropdown functionality
function initializeDropdowns() {
    const dropdownButtons = document.querySelectorAll('.dropdown-toggle');
    
    dropdownButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const dropdown = this.nextElementSibling;
            
            // Close other dropdowns
            document.querySelectorAll('.dropdown-menu').forEach(menu => {
                if (menu !== dropdown) {
                    menu.style.display = 'none';
                }
            });
            
            // Toggle current dropdown
            if (dropdown) {
                dropdown.style.display = dropdown.style.display === 'block' ? 'none' : 'block';
            }
        });
    });
    
    // Close dropdowns when clicking outside
    document.addEventListener('click', function(e) {
        if (!e.target.closest('.dropdown')) {
            document.querySelectorAll('.dropdown-menu').forEach(menu => {
                menu.style.display = 'none';
            });
        }
    });
}

// Wishlist functionality
function initializeWishlist() {
    const wishlistButtons = document.querySelectorAll('.btn-wishlist');
    
    wishlistButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const icon = this.querySelector('i');
            
            if (icon.classList.contains('fa-heart-o')) {
                icon.classList.remove('fa-heart-o');
                icon.classList.add('fa-heart');
                this.style.color = '#e74c3c';
                showFilterNotification('Đã thêm vào danh sách yêu thích!');
            } else {
                icon.classList.remove('fa-heart');
                icon.classList.add('fa-heart-o');
                this.style.color = '';
                showFilterNotification('Đã xóa khỏi danh sách yêu thích!');
            }
        });
    });
}

// Filter notification
function showFilterNotification(message) {
    const notification = document.createElement('div');
    notification.className = 'filter-notification';
    notification.innerHTML = `
        <i class="fa fa-check-circle"></i>
        <span>${message}</span>
    `;
    
    Object.assign(notification.style, {
        position: 'fixed',
        top: '20px',
        right: '20px',
        background: '#27ae60',
        color: 'white',
        padding: '15px 20px',
        borderRadius: '5px',
        zIndex: '10000',
        boxShadow: '0 4px 15px rgba(0,0,0,0.2)',
        transform: 'translateX(100%)',
        transition: 'transform 0.3s ease',
        display: 'flex',
        alignItems: 'center',
        gap: '10px',
        fontWeight: '600'
    });
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.style.transform = 'translateX(0)';
    }, 100);
    
    setTimeout(() => {
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }, 3000);
}

// Enhanced product hover effects
document.addEventListener('DOMContentLoaded', function() {
    const productCards = document.querySelectorAll('.products');
    
    productCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-5px)';
            this.style.transition = 'all 0.3s ease';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });
});

// Smooth scroll to top when filters change
function smoothScrollToTop() {
    window.scrollTo({
        top: 0,
        behavior: 'smooth'
    });
}

// Call smooth scroll when page loads with filters
document.addEventListener('DOMContentLoaded', function() {
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.has('minPrice') || urlParams.has('maxPrice') || urlParams.has('minRating') || urlParams.has('sortBy')) {
        setTimeout(smoothScrollToTop, 100);
    }
});

// ==============================================
// PRODUCT DETAIL PAGE FUNCTIONALITY
// ==============================================

document.addEventListener('DOMContentLoaded', function() {
    // Initialize detail page if exists
    if (document.querySelector('.single-product')) {
        initializeProductDetail();
    }
});

function initializeProductDetail() {
    // Initialize gallery
    initializeProductGallery();
    
    // Initialize quantity controls
    initializeQuantityControls();
    
    // Initialize variant selection (if variants exist)
    if (document.querySelector('.variant-option')) {
        initializeVariantSelection();
    }
    
    // Initialize tabs
    initializeProductTabs();
    
    // Initialize add to cart
    initializeAddToCart();
    
    // Initialize review system
    initializeReviewSystem();
    
    // Initialize animations
    initializeDetailAnimations();
}

// Product Gallery Functions
function initializeProductGallery() {
    const mainImage = document.querySelector('.single-product-gallery img');
    const thumbnails = document.querySelectorAll('.horizontal-thumb');
    
    if (!mainImage || !thumbnails.length) return;
    
    thumbnails.forEach(thumb => {
        thumb.addEventListener('click', function(e) {
            e.preventDefault();
            
            // Remove active class from all thumbnails
            thumbnails.forEach(t => t.classList.remove('active'));
            
            // Add active class to clicked thumbnail
            this.classList.add('active');
            
            // Update main image
            const newImageSrc = this.querySelector('img').src;
            const newImageAlt = this.querySelector('img').alt;
            
            // Fade out effect
            mainImage.style.opacity = '0.5';
            
            setTimeout(() => {
                mainImage.src = newImageSrc;
                mainImage.alt = newImageAlt;
                mainImage.style.opacity = '1';
            }, 200);
        });
    });
    
    // Add zoom functionality on hover
    mainImage.addEventListener('mouseenter', function() {
        this.style.cursor = 'zoom-in';
    });
    
    mainImage.addEventListener('mousemove', function(e) {
        const rect = this.getBoundingClientRect();
        const x = ((e.clientX - rect.left) / rect.width) * 100;
        const y = ((e.clientY - rect.top) / rect.height) * 100;
        
        this.style.transformOrigin = `${x}% ${y}%`;
    });
}

// Quantity Controls
function initializeQuantityControls() {
    const quantityInput = document.querySelector('input[name="quantity"]');
    
    if (!quantityInput) return;
    
    // Create quantity buttons if they don't exist
    const quantityContainer = quantityInput.parentElement;
    
    if (!quantityContainer.querySelector('.btn-number')) {
        const minusBtn = document.createElement('button');
        minusBtn.type = 'button';
        minusBtn.className = 'btn btn-outline-secondary btn-number';
        minusBtn.setAttribute('data-type', 'minus');
        minusBtn.setAttribute('data-field', 'quantity');
        minusBtn.innerHTML = '<i class="fa fa-minus"></i>';
        
        const plusBtn = document.createElement('button');
        plusBtn.type = 'button';
        plusBtn.className = 'btn btn-outline-secondary btn-number';
        plusBtn.setAttribute('data-type', 'plus');
        plusBtn.setAttribute('data-field', 'quantity');
        plusBtn.innerHTML = '<i class="fa fa-plus"></i>';
        
        // Wrap input in input-group
        const inputGroup = document.createElement('div');
        inputGroup.className = 'input-group quantity-controls';
        inputGroup.style.maxWidth = '150px';
        
        const inputGroupPrepend = document.createElement('div');
        inputGroupPrepend.className = 'input-group-prepend';
        inputGroupPrepend.appendChild(minusBtn);
        
        const inputGroupAppend = document.createElement('div');
        inputGroupAppend.className = 'input-group-append';
        inputGroupAppend.appendChild(plusBtn);
        
        quantityContainer.insertBefore(inputGroup, quantityInput);
        inputGroup.appendChild(inputGroupPrepend);
        inputGroup.appendChild(quantityInput);
        inputGroup.appendChild(inputGroupAppend);
    }
    
    // Handle quantity button clicks
    document.addEventListener('click', function(e) {
        if (e.target.closest('.btn-number')) {
            e.preventDefault();
            
            const button = e.target.closest('.btn-number');
            const type = button.getAttribute('data-type');
            const input = document.querySelector('input[name="quantity"]');
            
            if (!input) return;
            
            let currentVal = parseInt(input.value) || 1;
            const min = parseInt(input.getAttribute('min')) || 1;
            const max = parseInt(input.getAttribute('max')) || 999;
            
            if (type === 'minus' && currentVal > min) {
                input.value = currentVal - 1;
            } else if (type === 'plus' && currentVal < max) {
                input.value = currentVal + 1;
            }
            
            // Trigger change event
            input.dispatchEvent(new Event('change'));
            
            // Button animation
            button.style.transform = 'scale(0.95)';
            setTimeout(() => {
                button.style.transform = 'scale(1)';
            }, 100);
        }
    });
    
    // Validate quantity input
    quantityInput.addEventListener('change', function() {
        const min = parseInt(this.getAttribute('min')) || 1;
        const max = parseInt(this.getAttribute('max')) || 999;
        let value = parseInt(this.value) || min;
        
        if (value < min) value = min;
        if (value > max) value = max;
        
        this.value = value;
    });
}

// Variant Selection (existing functionality enhanced)
function initializeVariantSelection() {
    const variantOptions = document.querySelectorAll('.variant-option');
    
    if (!variantOptions.length) return;
    
    // Add enhanced hover effects
    variantOptions.forEach(option => {
        option.addEventListener('mouseenter', function() {
            if (!this.classList.contains('active')) {
                this.style.background = '#e9ecef';
                this.style.borderColor = '#5a88ca';
                this.style.transform = 'translateY(-1px)';
            }
        });
        
        option.addEventListener('mouseleave', function() {
            if (!this.classList.contains('active')) {
                this.style.background = '#fafafa';
                this.style.borderColor = '#ccc';
                this.style.transform = 'translateY(0)';
            }
        });
        
        // Add click animation
        option.addEventListener('click', function() {
            this.style.transform = 'scale(0.95)';
            setTimeout(() => {
                this.style.transform = this.classList.contains('active') ? 'translateY(0)' : 'translateY(0)';
            }, 100);
        });
    });
}

// Product Tabs
function initializeProductTabs() {
    const tabLinks = document.querySelectorAll('#product-tabs a[data-toggle="tab"]');
    const tabPanes = document.querySelectorAll('.tab-pane');
    
    if (!tabLinks.length) return;
    
    tabLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            
            const targetId = this.getAttribute('href');
            const targetPane = document.querySelector(targetId);
            
            if (!targetPane) return;
            
            // Remove active classes
            tabLinks.forEach(l => l.parentElement.classList.remove('active'));
            tabPanes.forEach(p => {
                p.classList.remove('active', 'in');
                p.style.display = 'none';
            });
            
            // Add active classes
            this.parentElement.classList.add('active');
            targetPane.classList.add('active', 'in');
            targetPane.style.display = 'block';
            
            // Animate tab content
            targetPane.style.opacity = '0';
            targetPane.style.transform = 'translateY(20px)';
            
            setTimeout(() => {
                targetPane.style.opacity = '1';
                targetPane.style.transform = 'translateY(0)';
                targetPane.style.transition = 'all 0.3s ease';
            }, 50);
        });
    });
}

// Add to Cart Enhancement
function initializeAddToCart() {
    const addToCartForm = document.getElementById('addToCartForm');
    const addToCartBtn = document.querySelector('button[type="submit"]');
    
    if (!addToCartForm || !addToCartBtn) return;
    
    // Enhanced button hover effect
    addToCartBtn.addEventListener('mouseenter', function() {
        this.style.transform = 'translateY(-2px)';
        this.style.boxShadow = '0 6px 20px rgba(90, 136, 202, 0.4)';
    });
    
    addToCartBtn.addEventListener('mouseleave', function() {
        this.style.transform = 'translateY(0)';
        this.style.boxShadow = '0 4px 15px rgba(90, 136, 202, 0.3)';
    });
    
    // Form submission with loading state
    addToCartForm.addEventListener('submit', function(e) {
        const selectedVariantId = document.getElementById('selectedVariantId');
        const attributes = document.querySelectorAll('.variant-option');
        
        // Check if variants are required but not selected
        if (attributes.length > 0 && (!selectedVariantId || !selectedVariantId.value)) {
            e.preventDefault();
            showDetailNotification('Vui lòng chọn đầy đủ phân loại sản phẩm!', 'error');
            return;
        }
        
        // Show loading state
        showDetailLoading(true);
        addToCartBtn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Đang thêm...';
        addToCartBtn.disabled = true;
        
        // Let form submit normally, loading will be hidden by page redirect
    });
}

// Review System Enhancement
function initializeReviewSystem() {
    // Initialize review form
    const reviewForm = document.querySelector('form[action*="AddReview"]');
    const replyForms = document.querySelectorAll('.reply-form');
    
    if (reviewForm) {
        reviewForm.addEventListener('submit', function(e) {
            const rating = this.querySelector('select[name="rating"]').value;
            const content = this.querySelector('textarea[name="content"]').value.trim();
            
            if (!content) {
                e.preventDefault();
                showDetailNotification('Vui lòng nhập nội dung đánh giá!', 'error');
                return;
            }
            
            if (content.length < 10) {
                e.preventDefault();
                showDetailNotification('Nội dung đánh giá phải có ít nhất 10 ký tự!', 'error');
                return;
            }
            
            // Show loading
            showDetailLoading(true);
            const submitBtn = this.querySelector('button[type="submit"]');
            submitBtn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Đang gửi...';
            submitBtn.disabled = true;
        });
    }
    
    // Initialize reply forms
    replyForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const replyContent = this.querySelector('input[name="replyContent"]').value.trim();
            
            if (!replyContent) {
                e.preventDefault();
                showDetailNotification('Vui lòng nhập nội dung phản hồi!', 'error');
                return;
            }
            
            // Show loading
            const submitBtn = this.querySelector('button[type="submit"]');
            submitBtn.innerHTML = '<i class="fa fa-spinner fa-spin"></i>';
            submitBtn.disabled = true;
        });
    });
    
    // Animate reviews on scroll
    const reviews = document.querySelectorAll('.review-item');
    if (reviews.length) {
        const reviewObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.style.opacity = '0';
                    entry.target.style.transform = 'translateX(-20px)';
                    entry.target.style.transition = 'all 0.5s ease';
                    
                    setTimeout(() => {
                        entry.target.style.opacity = '1';
                        entry.target.style.transform = 'translateX(0)';
                    }, 100);
                    
                    reviewObserver.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.1
        });
        
        reviews.forEach((review, index) => {
            review.style.animationDelay = `${index * 0.1}s`;
            reviewObserver.observe(review);
        });
    }
}

// Detail Page Animations
function initializeDetailAnimations() {
    // Animate elements on page load
    const animatedElements = document.querySelectorAll('.product-info, .gallery-holder, .product-tabs');
    
    animatedElements.forEach((element, index) => {
        element.style.opacity = '0';
        element.style.transform = 'translateY(30px)';
        element.style.transition = 'all 0.6s ease';
        
        setTimeout(() => {
            element.style.opacity = '1';
            element.style.transform = 'translateY(0)';
        }, 200 + (index * 200));
    });
    
    // Price animation
    const priceElement = document.querySelector('.price-box .price');
    if (priceElement) {
        priceElement.addEventListener('transitionend', function() {
            this.style.animation = 'priceHighlight 0.5s ease';
        });
    }
    
    // Breadcrumb animation
    const breadcrumbItems = document.querySelectorAll('.breadcrumb-inner li');
    breadcrumbItems.forEach((item, index) => {
        item.style.opacity = '0';
        item.style.transform = 'translateX(-20px)';
        item.style.transition = 'all 0.3s ease';
        
        setTimeout(() => {
            item.style.opacity = '1';
            item.style.transform = 'translateX(0)';
        }, 100 + (index * 100));
    });
}

// Utility Functions for Detail Page
function showDetailLoading(show = true) {
    let loadingEl = document.querySelector('.detail-loading');
    
    if (!loadingEl) {
        loadingEl = document.createElement('div');
        loadingEl.className = 'detail-loading';
        loadingEl.innerHTML = '<div class="spinner"></div>';
        document.body.appendChild(loadingEl);
    }
    
    if (show) {
        loadingEl.classList.add('active');
    } else {
        loadingEl.classList.remove('active');
    }
}

function showDetailNotification(message, type = 'success') {
    // Remove existing notifications
    document.querySelectorAll('.detail-toast').forEach(toast => toast.remove());
    
    const toast = document.createElement('div');
    toast.className = `detail-toast ${type}`;
    
    const icon = type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle';
    toast.innerHTML = `
        <i class="fa ${icon}"></i>
        <span>${message}</span>
    `;
    
    document.body.appendChild(toast);
    
    // Show toast
    setTimeout(() => {
        toast.classList.add('show');
    }, 100);
    
    // Hide toast after 3 seconds
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 300);
    }, 3000);
}

// Price animation keyframes
const priceAnimationStyle = document.createElement('style');
priceAnimationStyle.textContent = `
@keyframes priceHighlight {
    0% { color: #e74c3c; }
    50% { color: #f39c12; transform: scale(1.05); }
    100% { color: #e74c3c; transform: scale(1); }
}
`;
document.head.appendChild(priceAnimationStyle);

// Enhanced variant selection with price update animation
$(document).ready(function() {
    if (typeof variants !== 'undefined') {
        $(document).on('click', '.variant-option', function() {
            // Existing variant logic will work
            // Add animation for price change
            setTimeout(() => {
                const priceElement = $('#mainProductPrice');
                if (priceElement.length) {
                    priceElement.addClass('price-update-animation');
                    setTimeout(() => {
                        priceElement.removeClass('price-update-animation');
                    }, 500);
                }
            }, 100);
        });
    }
});

// Add CSS for price update animation
const priceUpdateStyle = document.createElement('style');
priceUpdateStyle.textContent = `
.price-update-animation {
    animation: priceUpdate 0.5s ease !important;
    transform-origin: left center;
}

@keyframes priceUpdate {
    0% { transform: scale(1); }
    50% { transform: scale(1.1); color: #f39c12; }
    100% { transform: scale(1); }
}
`;
document.head.appendChild(priceUpdateStyle);

// ==============================================
// UPSELL PRODUCTS FUNCTIONALITY
// ==============================================

document.addEventListener('DOMContentLoaded', function() {
    // Initialize upsell products functionality
    if (document.querySelector('.upsell-products-section')) {
        initializeUpsellProducts();
    }
});

function initializeUpsellProducts() {
    // Add to cart functionality for upsell products
    const addToCartButtons = document.querySelectorAll('.upsell-products-section .btn-add-to-cart');
    
    addToCartButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            
            const productId = this.getAttribute('data-product-id');
            const card = this.closest('.upsell-product-card');
            
            if (!productId) return;
            
            // Add loading state
            card.classList.add('loading');
            this.disabled = true;
            
            // Simulate add to cart (replace with actual AJAX call)
            setTimeout(() => {
                // Remove loading state
                card.classList.remove('loading');
                this.disabled = false;
                
                // Show success message
                showUpsellNotification('Đã thêm sản phẩm vào giỏ hàng!', 'success');
                
                // Update cart count if exists
                updateCartCount();
                
                // Add success animation
                this.innerHTML = '<i class="fa fa-check me-2"></i>Đã thêm';
                this.style.background = '#27ae60';
                
                setTimeout(() => {
                    this.innerHTML = '<i class="fa fa-shopping-cart me-2"></i>Thêm vào giỏ';
                    this.style.background = '';
                }, 2000);
                
            }, 1000);
        });
    });
    
    // Wishlist functionality
    const wishlistButtons = document.querySelectorAll('.upsell-products-section .btn-wishlist');
    
    wishlistButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            
            const isActive = this.classList.contains('active');
            
            if (isActive) {
                this.classList.remove('active');
                this.style.background = '#fff';
                this.style.color = '#333';
                showUpsellNotification('Đã xóa khỏi danh sách yêu thích', 'info');
            } else {
                this.classList.add('active');
                this.style.background = '#e74c3c';
                this.style.color = '#fff';
                showUpsellNotification('Đã thêm vào danh sách yêu thích', 'success');
            }
            
            // Animation effect
            this.style.transform = 'scale(0.8)';
            setTimeout(() => {
                this.style.transform = 'scale(1)';
            }, 150);
        });
    });
    
    // Compare functionality
    const compareButtons = document.querySelectorAll('.upsell-products-section .btn-compare');
    
    compareButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            
            const isActive = this.classList.contains('active');
            
            if (isActive) {
                this.classList.remove('active');
                this.style.background = '#fff';
                this.style.color = '#333';
                showUpsellNotification('Đã xóa khỏi danh sách so sánh', 'info');
            } else {
                this.classList.add('active');
                this.style.background = '#5a88ca';
                this.style.color = '#fff';
                showUpsellNotification('Đã thêm vào danh sách so sánh', 'success');
            }
        });
    });
    
    // Quick view functionality
    const quickViewButtons = document.querySelectorAll('.upsell-products-section .btn-quick-view');
    
    quickViewButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            
            const productId = this.getAttribute('data-product-id');
            if (!productId) return;
            
            // Open quick view modal (implement as needed)
            showQuickViewModal(productId);
        });
    });
    
    // Animate cards on scroll
    const cards = document.querySelectorAll('.upsell-product-card');
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const cardObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '0';
                entry.target.style.transform = 'translateY(30px)';
                entry.target.style.transition = 'all 0.6s ease';
                
                setTimeout(() => {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                }, 100);
                
                cardObserver.unobserve(entry.target);
            }
        });
    }, observerOptions);
    
    cards.forEach(card => {
        cardObserver.observe(card);
    });
}

// Enhanced UPSELL PRODUCTS CAROUSEL
document.addEventListener('DOMContentLoaded', function() {
    // Initialize enhanced upsell products carousel
    if (document.querySelector('.upsell-products-section')) {
        initializeEnhancedUpsellCarousel();
    }
});

function initializeEnhancedUpsellCarousel() {
    const track = document.getElementById('upsellTrack');
    const wrapper = document.getElementById('upsellWrapper');
    const prevBtn = document.getElementById('upsellPrev');
    const nextBtn = document.getElementById('upsellNext');
    const pagination = document.getElementById('upsellPagination');
    const items = document.querySelectorAll('.upsell-item');
    
    if (!track || !items.length) return;
    
    let currentIndex = 0;
    let itemsPerView = getItemsPerView();
    let totalPages = Math.ceil(items.length / itemsPerView);
    let isTransitioning = false;
    let autoPlayInterval;
    let itemWidth = 0;
    
    // Initialize carousel
    initializeCarousel();
    
    function initializeCarousel() {
        calculateItemWidth();
        updateItemsPerView();
        createPaginationDots();
        updateNavigation();
        startAutoPlay();
        
        // Add event listeners
        if (prevBtn) prevBtn.addEventListener('click', goToPrevious);
        if (nextBtn) nextBtn.addEventListener('click', goToNext);
        
        // Handle window resize
        window.addEventListener('resize', debounce(handleResize, 300));
        
        // Pause auto-play on hover
        wrapper.addEventListener('mouseenter', pauseAutoPlay);
        wrapper.addEventListener('mouseleave', startAutoPlay);
        
        // Touch/swipe support
        addTouchSupport();
        
        // Initialize existing upsell functionality
        initializeUpsellProducts();
    }
    
    function calculateItemWidth() {
        if (items.length > 0) {
            const containerWidth = wrapper.offsetWidth;
            const gap = 20;
            itemWidth = (containerWidth - (gap * (itemsPerView - 1))) / itemsPerView;
            
            // Set item widths
            items.forEach(item => {
                item.style.width = `${itemWidth}px`;
                item.style.flexShrink = '0';
            });
        }
    }
    
    function getItemsPerView() {
        const width = window.innerWidth;
        if (width >= 1200) return Math.min(6, items.length);
        if (width >= 992) return Math.min(4, items.length);
        if (width >= 768) return Math.min(3, items.length);
        if (width >= 576) return Math.min(2, items.length);
        return Math.min(2, items.length);
    }
    
    function updateItemsPerView() {
        itemsPerView = getItemsPerView();
        totalPages = Math.ceil(items.length / itemsPerView);
        
        // Reset to first page if current page is out of bounds
        if (currentIndex >= totalPages) {
            currentIndex = 0;
        }
        
        calculateItemWidth();
        updateCarouselPosition();
        createPaginationDots();
        updateNavigation();
    }
    
    function createPaginationDots() {
        if (!pagination || totalPages <= 1) {
            if (pagination) pagination.style.display = 'none';
            return;
        }
        
        pagination.style.display = 'block';
        pagination.innerHTML = '';
        
        for (let i = 0; i < totalPages; i++) {
            const dot = document.createElement('button');
            dot.className = `pagination-dot ${i === currentIndex ? 'active' : ''}`;
            dot.setAttribute('data-page', i);
            dot.addEventListener('click', () => goToPage(i));
            pagination.appendChild(dot);
        }
    }
    
    function updateCarouselPosition(animated = true) {
        if (isTransitioning || !track) return;
        
        const moveDistance = currentIndex * (itemWidth + 20);
        
        if (animated) {
            track.style.transition = 'transform 0.4s ease';
            isTransitioning = true;
            
            setTimeout(() => {
                isTransitioning = false;
            }, 400);
        } else {
            track.style.transition = 'none';
        }
        
        track.style.transform = `translateX(-${moveDistance}px)`;
    }
    
    function updateNavigation() {
        if (prevBtn) {
            prevBtn.disabled = currentIndex === 0 || items.length <= itemsPerView;
        }
        if (nextBtn) {
            nextBtn.disabled = currentIndex >= totalPages - 1 || items.length <= itemsPerView;
        }
        
        // Update pagination dots
        const dots = pagination.querySelectorAll('.pagination-dot');
        dots.forEach((dot, index) => {
            dot.classList.toggle('active', index === currentIndex);
        });
    }
    
    function goToPrevious() {
        if (currentIndex > 0 && !isTransitioning) {
            currentIndex--;
            updateCarouselPosition();
            updateNavigation();
            resetAutoPlay();
        }
    }
    
    function goToNext() {
        if (currentIndex < totalPages - 1 && !isTransitioning) {
            currentIndex++;
            updateCarouselPosition();
            updateNavigation();
            resetAutoPlay();
        }
    }
    
    function goToPage(pageIndex) {
        if (pageIndex !== currentIndex && !isTransitioning && pageIndex >= 0 && pageIndex < totalPages) {
            currentIndex = pageIndex;
            updateCarouselPosition();
            updateNavigation();
            resetAutoPlay();
        }
    }
    
    function handleResize() {
        updateItemsPerView();
    }
    
    // Auto-play functionality
    function startAutoPlay() {
        if (totalPages <= 1 || items.length <= itemsPerView) return;
        
        clearInterval(autoPlayInterval);
        autoPlayInterval = setInterval(() => {
            if (currentIndex < totalPages - 1) {
                goToNext();
            } else {
                // Loop back to first page
                currentIndex = 0;
                updateCarouselPosition();
                updateNavigation();
            }
        }, 5000); // Change slide every 5 seconds
    }
    
    function pauseAutoPlay() {
        clearInterval(autoPlayInterval);
    }
    
    function resetAutoPlay() {
        pauseAutoPlay();
        startAutoPlay();
    }
    
    // Touch/Swipe support
    function addTouchSupport() {
        let startX = 0;
        let endX = 0;
        let startY = 0;
        let endY = 0;
        
        wrapper.addEventListener('touchstart', (e) => {
            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
        }, { passive: true });
        
        wrapper.addEventListener('touchend', (e) => {
            endX = e.changedTouches[0].clientX;
            endY = e.changedTouches[0].clientY;
            handleSwipe();
        }, { passive: true });
        
        function handleSwipe() {
            const diffX = startX - endX;
            const diffY = startY - endY;
            
            // Only handle horizontal swipes
            if (Math.abs(diffX) > Math.abs(diffY) && Math.abs(diffX) > 50) {
                if (diffX > 0) {
                    // Swipe left - go to next
                    goToNext();
                } else {
                    // Swipe right - go to previous
                    goToPrevious();
                }
            }
        }
    }
    
    // Keyboard navigation
    document.addEventListener('keydown', (e) => {
        if (document.activeElement === prevBtn || document.activeElement === nextBtn) {
            if (e.key === 'ArrowLeft') {
                e.preventDefault();
                goToPrevious();
            } else if (e.key === 'ArrowRight') {
                e.preventDefault();
                goToNext();
            }
        }
    });
}

// Enhanced notification for upsell products
function showUpsellNotification(message, type = 'success') {
    // Remove existing notifications
    document.querySelectorAll('.upsell-notification').forEach(notification => {
        notification.remove();
    });
    
    const notification = document.createElement('div');
    notification.className = `upsell-notification`;
    
    const bgColor = type === 'success' ? '#27ae60' : 
                   type === 'error' ? '#e74c3c' : '#3498db';
    
    const icon = type === 'success' ? 'fa-check-circle' : 
                 type === 'error' ? 'fa-exclamation-circle' : 'fa-info-circle';
    
    notification.innerHTML = `
        <i class="fa ${icon}"></i>
        <span>${message}</span>
    `;
    
    // Style the notification
    Object.assign(notification.style, {
        position: 'fixed',
        top: '20px',
        right: '20px',
        background: bgColor,
        color: '#fff',
        padding: '15px 20px',
        borderRadius: '8px',
        boxShadow: '0 4px 20px rgba(0,0,0,0.15)',
        zIndex: '10000',
        transform: 'translateX(100%)',
        transition: 'transform 0.3s ease',
        display: 'flex',
        alignItems: 'center',
        gap: '10px',
        fontSize: '14px',
        fontWeight: '600',
        minWidth: '300px'
    });
    
    document.body.appendChild(notification);
    
    // Show notification
    setTimeout(() => {
        notification.style.transform = 'translateX(0)';
    }, 100);
    
    // Hide notification after 3 seconds
    setTimeout(() => {
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }, 3000);
}

// Utility function for debouncing
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// ==============================================
// END OF ENHANCED UPSELL PRODUCTS CAROUSEL
// ==============================================