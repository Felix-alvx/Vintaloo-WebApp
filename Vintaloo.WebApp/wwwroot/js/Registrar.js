function validarCampo(input, minLen) {
    if (input.value.length >= minLen) {
        input.classList.add('valid');
        input.classList.remove('invalid');
    } else if (input.value.length > 0) {
        input.classList.add('invalid');
        input.classList.remove('valid');
    } else {
        input.classList.remove('valid', 'invalid');
    }
}

function validarCorreo(input) {
    const fb = document.getElementById('correoFeedback');
    const ok = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(input.value);
    if (input.value.length === 0) {
        input.classList.remove('valid', 'invalid');
        fb.textContent = '';
        return;
    }
    if (ok) {
        input.classList.add('valid');
        input.classList.remove('invalid');
        fb.textContent = '¡Correo válido!';
        fb.className = 'field-feedback feedback-ok';
    } else {
        input.classList.add('invalid');
        input.classList.remove('valid');
        fb.textContent = 'Ingresa un correo válido.';
        fb.className = 'field-feedback feedback-err';
    }
}

function medirFuerza(input) {
    const pwd = input.value;
    const bars = [document.getElementById('bar1'),
    document.getElementById('bar2'),
    document.getElementById('bar3')];
    const label = document.getElementById('pwdLabel');

    bars.forEach(b => b.className = 'pwd-bar');

    if (pwd.length === 0) { label.textContent = ''; return; }

    let score = 0;
    if (pwd.length >= 6) score++;
    if (/[A-Z]/.test(pwd) && /[0-9]/.test(pwd)) score++;
    if (/[^A-Za-z0-9]/.test(pwd)) score++;

    if (score === 1) {
        bars[0].classList.add('weak');
        label.textContent = 'Débil';
        label.style.color = 'var(--error)';
    } else if (score === 2) {
        bars[0].classList.add('medium');
        bars[1].classList.add('medium');
        label.textContent = 'Media';
        label.style.color = 'var(--terra)';
    } else {
        bars.forEach(b => b.classList.add('strong'));
        label.textContent = 'Fuerte';
        label.style.color = 'var(--success)';
    }
}

function validarConfirmar(input) {
    const fb = document.getElementById('confirmarFeedback');
    const pwd = document.getElementById('password').value;
    if (input.value.length === 0) {
        input.classList.remove('valid', 'invalid');
        fb.textContent = '';
        return;
    }
    if (input.value === pwd) {
        input.classList.add('valid');
        input.classList.remove('invalid');
        fb.textContent = 'Las contraseñas coinciden.';
        fb.className = 'field-feedback feedback-ok';
    } else {
        input.classList.add('invalid');
        input.classList.remove('valid');
        fb.textContent = 'Las contraseñas no coinciden.';
        fb.className = 'field-feedback feedback-err';
    }
}

// Evitar submit si las contraseñas no coinciden
document.getElementById('formRegistro').addEventListener('submit', function (e) {
    const pwd = document.getElementById('password').value;
    const confirmar = document.getElementById('confirmar').value;
    if (pwd !== confirmar) {
        e.preventDefault();
        document.getElementById('confirmarFeedback').textContent = 'Las contraseñas no coinciden.';
        document.getElementById('confirmarFeedback').className = 'field-feedback feedback-err';
        document.getElementById('confirmar').classList.add('invalid');
    }
});