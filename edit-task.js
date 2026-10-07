(() => {
  const $ = s => document.querySelector(s);
  const $$ = s => [...document.querySelectorAll(s)];
  const form = $('#postTaskForm');
  let currentStep = 1;
  const totalSteps = 4;
  const taskKey = new URLSearchParams(location.search).get('task') || 'fix-leaking-faucet';
  const toast = msg => {
    const el = $('#toast'); if (!el) return;
    el.textContent = msg; el.classList.add('show'); clearTimeout(window.__editToast);
    window.__editToast = setTimeout(() => el.classList.remove('show'), 2400);
  };
  const money = value => `₱${Number(value || 0).toLocaleString('en-PH')}`;
  const fields = ['taskTitle','category','taskType','taskStatus','description','requirements','amount','paymentMethod','budgetType','deadline','startTime','duration','barangay','scheduleNotes'];
  const seedTasks = {
    'fix-leaking-faucet': {taskTitle:'Fix a leaking faucet',category:'Repairs',taskType:'One-time task',taskStatus:'active',description:'Need help fixing a leaking kitchen faucet. The faucet drips continuously even when turned off. Please check the connection, replace the worn part if needed, and make sure there are no leaks after the repair.',requirements:'Basic plumbing experience\nBring basic hand tools\nClean up the work area after the repair',amount:'600',paymentMethod:'Cash',budgetType:'Fixed total',deadline:'2026-10-08',startTime:'09:00',duration:'1–2 hours',barangay:'176',scheduleNotes:'Please message before coming. Someone will be home after 9:00 AM.'},
    'house-cleaning': {taskTitle:'House Cleaning',category:'Cleaning',taskType:'One-time task',taskStatus:'active',description:'General cleaning for a small home, including sweeping, mopping, dusting, and kitchen cleanup.',requirements:'Bring basic cleaning supplies if available',amount:'500',paymentMethod:'Cash',budgetType:'Fixed total',deadline:'2026-10-07',startTime:'10:00',duration:'2–3 hours',barangay:'176',scheduleNotes:'Please message before coming.'},
    'math-tutor': {taskTitle:'Math Tutor (Grade 10)',category:'Tutoring',taskType:'Short-term / recurring',taskStatus:'active',description:'Looking for a patient tutor for Grade 10 math topics and homework support.',requirements:'Comfortable explaining algebra and geometry',amount:'300',paymentMethod:'Cash',budgetType:'Hourly rate',deadline:'2026-10-10',startTime:'16:00',duration:'1–2 hours',barangay:'178',scheduleNotes:'Weekday afternoons preferred.'}
  };
  const getData = () => Object.fromEntries(fields.map(id => [id, $('#'+id)?.value || '']));
  const setData = data => fields.forEach(id => { if ($('#'+id) && data[id] != null) $('#'+id).value = data[id]; });
  const formatDate = value => value ? new Date(`${value}T00:00:00`).toLocaleDateString('en-PH',{month:'short',day:'numeric',year:'numeric'}) : '—';

  const updateStatusUI = () => {
    const paused = $('#taskStatus')?.value === 'paused';
    $('#pauseToggle')?.classList.toggle('on', !paused);
    $('#pauseToggle')?.classList.toggle('off', paused);
    const statusEls = $$('.preview-status, .status-pill');
    statusEls.forEach(el => { el.textContent = paused ? 'Paused' : 'Published'; el.classList.toggle('paused', paused); el.classList.toggle('published', !paused); });
  };
  const updatePreview = () => {
    const d = getData();
    $('#previewTitle').textContent = d.taskTitle.trim() || 'Your task title';
    $('#previewPrice').textContent = money(d.amount);
    $('#previewCategory').textContent = d.category || 'Category';
    $('#previewArea').textContent = d.barangay ? `Brgy. ${d.barangay}` : 'Barangay';
    $('#previewDate').textContent = formatDate(d.deadline);
    $('#previewDescription').textContent = d.description.trim() || 'Your task description will appear here as you edit it.';
    $('#budgetPreview').textContent = money(d.amount);
    $('#reviewTitle').textContent = d.taskTitle.trim() || '—';
    $('#reviewBudget').textContent = money(d.amount);
    $('#reviewArea').textContent = d.barangay ? `Brgy. ${d.barangay}` : '—';
    $('#reviewDeadline').textContent = formatDate(d.deadline);
    $('#modalTitle').textContent = d.taskTitle.trim() || 'Untitled task';
    $('#modalBudget').textContent = money(d.amount);
    $('#modalArea').textContent = d.barangay ? `Brgy. ${d.barangay}` : '—';
    updateStatusUI();
  };
  const updateStepper = () => {
    $$('.step').forEach(btn => { const n=Number(btn.dataset.step); btn.classList.toggle('active',n===currentStep); btn.classList.toggle('done',n<currentStep); });
    $$('.step-panel').forEach(panel => { panel.hidden=Number(panel.dataset.panel)!==currentStep; panel.classList.toggle('active',Number(panel.dataset.panel)===currentStep); });
    $('#backBtn').hidden=currentStep===1; $('#nextBtn').hidden=currentStep===totalSteps; $('#publishBtn').hidden=currentStep!==totalSteps;
    window.scrollTo({top:0,behavior:'smooth'}); updatePreview();
  };
  const rules = {1:[['taskTitle','Please add a task title.'],['category','Choose a category.']],2:[['description','Please describe what needs to be done.']],3:[['amount','Enter a compensation amount.'],['paymentMethod','Choose a payment method.']],4:[['deadline','Choose a deadline.'],['barangay','Choose the task area.']]};
  const validateStep = step => {
    let ok=true;
    (rules[step]||[]).forEach(([id,msg])=>{const el=$('#'+id),wrap=el?.closest('.field');const bad=!el||!el.value.trim()||(id==='amount'&&Number(el.value)<=0);if(bad){ok=false;wrap?.classList.add('invalid');el?.setAttribute('aria-invalid','true');if(wrap&&!wrap.querySelector('.error-msg')){const err=document.createElement('small');err.className='error-msg';err.textContent=msg;wrap.appendChild(err);}}else{wrap?.classList.remove('invalid');wrap?.querySelector('.error-msg')?.remove();el?.removeAttribute('aria-invalid');}});
    if(!ok) toast('Please complete the required fields before continuing.'); return ok;
  };
  $('#nextBtn')?.addEventListener('click',()=>{if(!validateStep(currentStep))return;currentStep++;updateStepper();});
  $('#backBtn')?.addEventListener('click',()=>{if(currentStep>1){currentStep--;updateStepper();}});
  $$('.step').forEach(btn=>btn.addEventListener('click',()=>{const target=Number(btn.dataset.step);if(target<currentStep){currentStep=target;updateStepper();}else if(target!==currentStep){let step=currentStep;while(step<target){if(!validateStep(step))return;step++;}currentStep=target;updateStepper();}}));
  $$('#postTaskForm input,#postTaskForm select,#postTaskForm textarea').forEach(el=>{el.addEventListener('input',()=>{el.closest('.field')?.classList.remove('invalid');el.closest('.field')?.querySelector('.error-msg')?.remove();updatePreview();});el.addEventListener('change',updatePreview);});
  $('#description')?.addEventListener('input',e=>{$('#descriptionCount').textContent=e.target.value.length;});
  $$('.suggested button').forEach(btn=>btn.addEventListener('click',()=>{$('#amount').value=btn.dataset.amount;updatePreview();}));

  const photoInput=$('#taskPhoto');
  photoInput?.addEventListener('change',()=>{const file=photoInput.files?.[0];if(!file)return;if(file.size>5*1024*1024){toast('That image is larger than 5 MB.');photoInput.value='';return;}$('#uploadBox').hidden=true;$('#filePreview').hidden=false;$('#fileName').textContent=file.name;$('#fileSize').textContent=`${(file.size/1024/1024).toFixed(2)} MB`;const reader=new FileReader();reader.onload=e=>{$('#fileThumb').style.backgroundImage=`url("${e.target.result}")`;$('#previewImage').style.backgroundImage=`url("${e.target.result}")`;$(`#previewImage`).style.backgroundSize='cover';$('#previewImage').style.backgroundPosition='center';$('#previewImage').innerHTML='<small class="photo-overlay">Task photo</small>';};reader.readAsDataURL(file);});
  $('#removeFile')?.addEventListener('click',()=>{photoInput.value='';$('#uploadBox').hidden=false;$('#filePreview').hidden=true;$('#previewImage').style.backgroundImage='';$('#previewImage').style.backgroundSize='';$('#previewImage').innerHTML='<span>＋</span><small>Task photo</small>';});
  $('#uploadBox')?.addEventListener('click',e=>{if(e.target.tagName!=='INPUT')photoInput?.click();});

  const storageKey=`tasktapEdit:${taskKey}`;
  $('#saveDraft')?.addEventListener('click',()=>{localStorage.setItem(storageKey,JSON.stringify(getData()));toast('Changes saved on this device.');});
  const saved=localStorage.getItem(storageKey);
  if(saved){try{setData(JSON.parse(saved));}catch(e){}}
  else setData(seedTasks[taskKey] || seedTasks['fix-leaking-faucet']);
  $('#descriptionCount').textContent=$('#description')?.value.length||0;

  const modal=$('#publishModal');
  form?.addEventListener('submit',e=>{e.preventDefault();if(!validateStep(4))return;$('#publishModal').hidden=false;document.body.classList.add('modal-open');});
  $('#confirmPublish')?.addEventListener('click',()=>{localStorage.removeItem(storageKey);localStorage.setItem('tasktapLastEdited',JSON.stringify({task:taskKey,updatedAt:new Date().toISOString(),...getData()}));$('#publishModal').hidden=true;document.body.classList.remove('modal-open');toast('Task changes saved successfully.');setTimeout(()=>location.href='dashboard.html?updated=1',1100);});
  const archive=()=>{$('#archiveModal').hidden=false;document.body.classList.add('modal-open');};
  $('#archiveTopBtn')?.addEventListener('click',archive);$('#archiveSideBtn')?.addEventListener('click',archive);
  $('#confirmArchive')?.addEventListener('click',()=>{localStorage.setItem('tasktapArchivedTask',JSON.stringify({task:taskKey,archivedAt:new Date().toISOString(),...getData()}));toast('Task archived.');$('#archiveModal').hidden=true;document.body.classList.remove('modal-open');setTimeout(()=>location.href='dashboard.html?archived=1',900);});
  $('#pauseBtn')?.addEventListener('click',()=>{const select=$('#taskStatus');select.value=select.value==='paused'?'active':'paused';updatePreview();toast(select.value==='paused'?'New requests paused.':'Task is accepting requests again.');});
  $$('[data-close]').forEach(btn=>btn.addEventListener('click',()=>{const id=btn.dataset.close;$('#'+id).hidden=true;document.body.classList.remove('modal-open');}));
  $$('.modal-backdrop').forEach(back=>back.addEventListener('click',e=>{if(e.target===back){back.hidden=true;document.body.classList.remove('modal-open');}}));
  document.addEventListener('keydown',e=>{if(e.key==='Escape'){$$('.modal-backdrop:not([hidden])').forEach(m=>m.hidden=true);document.body.classList.remove('modal-open');}});
  updateStepper();
})();
