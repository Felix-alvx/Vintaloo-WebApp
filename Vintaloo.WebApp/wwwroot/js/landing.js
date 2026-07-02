document.addEventListener('DOMContentLoaded', function () {

    // ── Animación de entrada para las secciones al hacer scroll ──
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
            }
        });
    }, { threshold: 0.15 });

    document.querySelectorAll('.paso-card, .impacto-content, .impacto-circle-wrap')
        .forEach(el => {
            el.classList.add('fade-on-scroll');
            observer.observe(el);
        });

    // ── Validación simple del formulario CTA ──
    const ctaInput = document.querySelector('.cta-input');
    const ctaBtn = document.querySelector('.cta-form .btn-primary-vt');

    if (ctaBtn && ctaInput) {
        ctaBtn.addEventListener('click', function (e) {
            e.preventDefault();
            const email = ctaInput.value.trim();

            if (!email || !email.includes('@')) {
                ctaInput.style.borderColor = '#e74c3c';
                ctaInput.placeholder = 'Ingresá un correo válido';
            } else {
                ctaInput.style.borderColor = '#2D4A3E';
                alert('¡Gracias! Te avisaremos pronto 🌿');
                ctaInput.value = '';
            }
        });

        // Resetea el borde al escribir
        ctaInput.addEventListener('input', function () {
            ctaInput.style.borderColor = 'rgba(0,0,0,0.12)';
        });
    }
});