// Documentation JavaScript
document.addEventListener('DOMContentLoaded', function() {
    initializeDocs();
});

function initializeDocs() {
    initializeSidebar();
    initializeCodeTabs();
    initializeCopyButtons();
    initializeFeedbackWidget();
    initializeKeyboardShortcuts();
    initializeSmoothScrolling();
    
    // Track page visit
    trackPageVisit();
}

// Sidebar functionality
function initializeSidebar() {
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('docsSidebar');
    
    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function() {
            sidebar.classList.toggle('open');
        });
        
        // Close sidebar when clicking outside on mobile
        document.addEventListener('click', function(e) {
            if (window.innerWidth <= 1024 && 
                !sidebar.contains(e.target) && 
                !sidebarToggle.contains(e.target) &&
                sidebar.classList.contains('open')) {
                sidebar.classList.remove('open');
            }
        });
    }
}



// Code tabs functionality
function initializeCodeTabs() {
    const tabButtons = document.querySelectorAll('.tab-button');
    
    tabButtons.forEach(button => {
        button.addEventListener('click', function() {
            const tabId = this.dataset.tab;
            const tabsContainer = this.closest('.code-tabs');
            
            // Update active tab button
            tabsContainer.querySelectorAll('.tab-button').forEach(btn => {
                btn.classList.remove('active');
            });
            this.classList.add('active');
            
            // Update active tab content
            tabsContainer.querySelectorAll('.tab-content').forEach(content => {
                content.classList.remove('active');
            });
            tabsContainer.querySelector(`#${tabId}`).classList.add('active');
        });
    });
}

// Copy code functionality
function initializeCopyButtons() {
    const copyButtons = document.querySelectorAll('.copy-button');
    
    copyButtons.forEach(button => {
        button.addEventListener('click', function() {
            const codeBlock = this.closest('.code-block, .code-tabs');
            let codeElement;
            
            if (codeBlock.classList.contains('code-tabs')) {
                const activeTab = codeBlock.querySelector('.tab-content.active');
                codeElement = activeTab.querySelector('pre');
            } else {
                codeElement = codeBlock.querySelector('pre');
            }
            
            if (codeElement) {
                const text = codeElement.textContent;
                copyToClipboard(text, this);
            }
        });
    });
}

function copyToClipboard(text, button) {
    navigator.clipboard.writeText(text).then(function() {
        // Show success feedback
        const originalHTML = button.innerHTML;
        button.innerHTML = '<i class="fas fa-check"></i> Copied!';
        button.style.background = 'var(--green-500)';
        
        setTimeout(() => {
            button.innerHTML = originalHTML;
            button.style.background = 'var(--primary-600)';
        }, 2000);
    }).catch(function(err) {
        console.error('Failed to copy text: ', err);
        // Fallback for older browsers
        const textArea = document.createElement('textarea');
        textArea.value = text;
        document.body.appendChild(textArea);
        textArea.select();
        document.execCommand('copy');
        document.body.removeChild(textArea);
        
        const originalHTML = button.innerHTML;
        button.innerHTML = '<i class="fas fa-check"></i> Copied!';
        button.style.background = 'var(--green-500)';
        
        setTimeout(() => {
            button.innerHTML = originalHTML;
            button.style.background = 'var(--primary-600)';
        }, 2000);
    });
}

// Page visit tracking
function trackPageVisit() {
    const visitData = {
        pageUrl: window.location.href,
        pageTitle: document.title,
        timestamp: new Date().toISOString()
    };

    fetch('/api/DataCollection/visit', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(visitData)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            console.log('Page visit tracked successfully');
        }
    })
    .catch(error => {
        console.error('Error tracking page visit:', error);
    });
}

// Feedback widget
function initializeFeedbackWidget() {
    const feedbackButtons = document.querySelectorAll('.feedback-btn');
    const feedbackForm = document.getElementById('feedbackForm');
    const feedbackSubmit = document.querySelector('.feedback-submit');
    
    feedbackButtons.forEach(button => {
        button.addEventListener('click', function() {
            const feedback = this.dataset.feedback;
            
            // Hide buttons and show form
            document.querySelector('.feedback-buttons').style.display = 'none';
            feedbackForm.style.display = 'block';
            
            // Store feedback type
            feedbackForm.dataset.feedback = feedback;
        });
    });
    
    if (feedbackSubmit) {
        feedbackSubmit.addEventListener('click', function() {
            const feedback = feedbackForm.dataset.feedback;
            const comment = feedbackForm.querySelector('textarea').value;
            
            // Send feedback to the server
            const feedbackData = {
                pageUrl: window.location.href,
                pageTitle: document.title,
                wasHelpful: feedback === 'positive',
                comment: comment || null,
                timestamp: new Date().toISOString()
            };

            fetch('/api/DataCollection/feedback', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(feedbackData)
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    console.log('Feedback submitted successfully');
                    
                    // Show thank you message
                    const feedbackWidget = document.getElementById('feedbackWidget');
                    feedbackWidget.innerHTML = `
                        <div class="feedback-content">
                            <h4>Thank you for your feedback!</h4>
                            <p>We appreciate your input to improve our documentation.</p>
                        </div>
                    `;
                    
                    // Hide feedback widget after 3 seconds
                    setTimeout(() => {
                        feedbackWidget.style.display = 'none';
                    }, 3000);
                } else {
                    console.error('Failed to submit feedback:', data.message);
                }
            })
            .catch(error => {
                console.error('Error submitting feedback:', error);
            });
        });
    }
}

// Keyboard shortcuts
function initializeKeyboardShortcuts() {
    document.addEventListener('keydown', function(e) {
        // Only handle shortcuts when not typing in input fields
        if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') {
            return;
        }
        
        // j/k navigation
        if (e.key === 'j' && !e.ctrlKey && !e.metaKey) {
            e.preventDefault();
            navigateSections(1);
        } else if (e.key === 'k' && !e.ctrlKey && !e.metaKey) {
            e.preventDefault();
            navigateSections(-1);
        }
        
        // / for search (placeholder)
        if (e.key === '/' && !e.ctrlKey && !e.metaKey) {
            e.preventDefault();
            // TODO: Implement search functionality
            console.log('Search functionality coming soon');
        }
        
        // T for theme toggle (placeholder)
        if (e.key === 't' && !e.ctrlKey && !e.metaKey) {
            e.preventDefault();
            // TODO: Implement theme toggle
            console.log('Theme toggle coming soon');
        }
    });
}

function navigateSections(direction) {
    const sections = document.querySelectorAll('.docs-section');
    const currentScroll = window.pageYOffset;
    
    let targetSection = null;
    
    if (direction > 0) {
        // Navigate down
        for (let section of sections) {
            if (section.offsetTop > currentScroll + 100) {
                targetSection = section;
                break;
            }
        }
    } else {
        // Navigate up
        for (let i = sections.length - 1; i >= 0; i--) {
            if (sections[i].offsetTop < currentScroll - 100) {
                targetSection = sections[i];
                break;
            }
        }
    }
    
    if (targetSection) {
        targetSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
}

// Smooth scrolling for anchor links
function initializeSmoothScrolling() {
    const links = document.querySelectorAll('a[href^="#"]');
    
    links.forEach(link => {
        link.addEventListener('click', function(e) {
            const href = this.getAttribute('href');
            if (href === '#') return;
            
            const target = document.querySelector(href);
            if (target) {
                e.preventDefault();
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
                
                // Update URL without page jump
                history.pushState(null, null, href);
            }
        });
    });
}

// Utility function to debounce scroll events
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

// Add loading animation
window.addEventListener('load', function() {
    document.body.classList.add('loaded');
});
