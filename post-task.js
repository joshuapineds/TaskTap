(() => {
  const $ = s => document.querySelector(s);
  const $$ = s => [...document.querySelectorAll(s)];
  const form = $('#postTaskForm');
  let currentStep = 1;
  const totalSteps = 4;
  const toast = msg => {
    const el = $('#toast'); if (!el) return;
    el.textContent = msg; el.classList.add('show'); clearTimeout(window.__postToast);
    window.__postToast = setTimeout(() => el.classList.remove('show'), 2400);
  };
  const money = value => `₱${Number(value || 0).toLocaleString('en-PH')}`;
  const fields = ['taskTitle','category','taskType','description','requirements','amount','paymentMethod','budgetType','deadline','startTime','duration','barangay','scheduleNotes'];
  const getData = () => Object.fromEntries(fields.map(id => [id, $('#' + id)?.value || '']));
  const setData = data => fields.forEach(id => { if ($('#'+id) && data[id] != null) $('#'+id).value = data[id]; });

  const updatePreview = () => {
    const d = getData();
    $('#previewTitle').textContent = d.taskTitle.trim() || 'Your task title';
    $('#previewPrice').textContent = money(d.amount);
    $('#previewCategory').textContent = d.category || 'Category';
    $('#previewArea').textContent = d.barangay ? `Brgy. ${d.barangay}` : 'Barangay';
    $('#previewDate').textContent = d.deadline ? new Date(`${d.deadline}T00:00:00`).toLocaleDateString('en-PH',{month:'short',day:'numeric',year:'numeric'}) : 'Deadline';
    $('#previewDescription').textContent = d.description.trim() || 'Your task description will appear here as you fill out the form.';
    $('#budgetPreview').textContent = money(d.amount);
    $('#reviewTitle').textContent = d.taskTitle.trim() || '—';
    $('#reviewBudget').textContent = money(d.amount);
    $('#reviewArea').textContent = d.barangay ? `Brgy. ${d.barangay}` : '—';
    $('#reviewDeadline').textContent = d.deadline ? new Date(`${d.deadline}T00:00:00`).toLocaleDateString('en-PH',{month:'short',day:'numeric',year:'numeric'}) : '—';
    $('#modalTitle').textContent = d.taskTitle.trim() || 'Untitled task';
    $('#modalBudget').textContent = money(d.amount);
    $('#modalArea').textContent = d.barangay ? `Brgy. ${d.barangay}` : '—';
  };

  const updateStepper = () => {
    $$('.step').forEach(btn => {
      const n = Number(btn.dataset.step);
      btn.classList.toggle('active', n === currentStep);
      btn.classList.toggle('done', n < currentStep);
    });
    $$('.step-panel').forEach(panel => {
      panel.hidden = Number(panel.dataset.panel) !== currentStep;
      panel.classList.toggle('active', Number(panel.dataset.panel) === currentStep);
    });
    $('#backBtn').hidden = currentStep === 1;
    $('#nextBtn').hidden = currentStep === totalSteps;
    $('#publishBtn').hidden = currentStep !== totalSteps;
    window.scrollTo({top: 0, behavior: 'smooth'});
    updatePreview();
  };

  const rules = {
    1: [['taskTitle','Please add a task title.'], ['category','Choose a category.']],
    2: [['description','Please describe what needs to be done.']],
    3: [['amount','Enter a compensation amount.'], ['paymentMethod','Choose a payment method.']],
    4: [['deadline','Choose a deadline.'], ['barangay','Choose the task area.']]
  };
  const validateStep = step => {
    let ok = true;
    (rules[step] || []).forEach(([id,msg]) => {
      const el = $('#'+id), wrap = el?.closest('.field');
      if (!el || !el.value.trim() || (id === 'amount' && Number(el.value) <= 0)) {
        ok = false; wrap?.classList.add('invalid');
        if (el) el.setAttribute('aria-invalid','true');
        if (wrap) { let err = wrap.querySelector('.error-msg'); if (!err) { err = document.createElement('small'); err.className='error-msg'; err.textContent=msg; wrap.appendChild(err); } }
      } else { wrap?.classList.remove('invalid'); wrap?.querySelector('.error-msg')?.remove(); el.removeAttribute('aria-invalid'); }
    });
    if (!ok) toast('Please complete the required fields before continuing.');
    return ok;
  };

  $('#nextBtn')?.addEventListener('click', () => { if (!validateStep(currentStep)) return; currentStep++; updateStepper(); });
  $('#backBtn')?.addEventListener('click', () => { if (currentStep > 1) { currentStep--; updateStepper(); } });
  $$('.step').forEach(btn => btn.addEventListener('click', () => {
    const target = Number(btn.dataset.step);
    if (target < currentStep) { currentStep = target; updateStepper(); }
    else if (target === currentStep) return;
    else {
      let step = currentStep;
      while (step < target) { if (!validateStep(step)) return; step++; }
      currentStep = target; updateStepper();
    }
  }));

  $$('#postTaskForm input, #postTaskForm select, #postTaskForm textarea').forEach(el => {
    el.addEventListener('input', () => { el.closest('.field')?.classList.remove('invalid'); el.closest('.field')?.querySelector('.error-msg')?.remove(); updatePreview(); });
    el.addEventListener('change', updatePreview);
  });
  $('#description')?.addEventListener('input', e => { $('#descriptionCount').textContent = e.target.value.length; });
  $$('.suggested button').forEach(btn => btn.addEventListener('click', () => { $('#amount').value = btn.dataset.amount; updatePreview(); }));

  const photoInput = $('#taskPhoto');
  photoInput?.addEventListener('change', () => {
    const file = photoInput.files?.[0]; if (!file) return;
    if (file.size > 5 * 1024 * 1024) { toast('That image is larger than 5 MB.'); photoInput.value=''; return; }
    $('#uploadBox').hidden = true; $('#filePreview').hidden = false; $('#fileName').textContent = file.name; $('#fileSize').textContent = `${(file.size/1024/1024).toFixed(2)} MB`;
    const reader = new FileReader(); reader.onload = e => { $('#fileThumb').style.backgroundImage = `url("${e.target.result}")`; $('#previewImage').style.backgroundImage = `url("${e.target.result}")`; $('#previewImage').style.backgroundSize='cover'; $('#previewImage').style.backgroundPosition='center'; $('#previewImage').innerHTML='<small class="photo-overlay">Task photo</small>'; }; reader.readAsDataURL(file);
  });
  $('#removeFile')?.addEventListener('click', () => { photoInput.value=''; $('#uploadBox').hidden=false; $('#filePreview').hidden=true; $('#previewImage').style.backgroundImage=''; $('#previewImage').style.backgroundSize=''; $('#previewImage').innerHTML='<span>＋</span><small>Task photo</small>'; });
  $('#uploadBox')?.addEventListener('click', e => { if (e.target.tagName !== 'INPUT') photoInput?.click(); });

  $('#saveDraft')?.addEventListener('click', () => {
    localStorage.setItem('tasktapPostDraft', JSON.stringify(getData()));
    toast('Draft saved on this device.');
  });
  const saved = localStorage.getItem('tasktapPostDraft');
  if (saved) { try { setData(JSON.parse(saved)); $('#descriptionCount').textContent = $('#description').value.length; updatePreview(); } catch(e) {} }

  form?.addEventListener('submit', e => {
    e.preventDefault();
    if (!validateStep(4)) return;
    $('#publishModal').hidden = false; document.body.classList.add('modal-open');
  });
  $('#confirmPublish')?.addEventListener('click', () => {
    localStorage.removeItem('tasktapPostDraft');
    $('#publishModal').hidden = true; document.body.classList.remove('modal-open');
    toast('Task published successfully.');
    setTimeout(() => { location.href = 'dashboard.html?posted=1'; }, 1100);
  });
  $$('[data-close]').forEach(btn => btn.addEventListener('click', () => { const id=btn.dataset.close; $('#'+id).hidden=true; document.body.classList.remove('modal-open'); }));
  $('#publishModal')?.addEventListener('click', e => { if (e.target.id === 'publishModal') { e.currentTarget.hidden=true; document.body.classList.remove('modal-open'); } });
  document.addEventListener('keydown', e => { if (e.key==='Escape' && !$('#publishModal').hidden) { $('#publishModal').hidden=true; document.body.classList.remove('modal-open'); } });
  updateStepper();
})();
