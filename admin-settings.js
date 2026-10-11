(() => {
  const el = id => document.getElementById(id);
  const defaults = {
    workspaceName: 'TaskTap PH', supportEmail: 'support@tasktap.demo', defaultArea: 'All service areas',
    demoBadge: true, maintenanceNotice: false, notifyReports: true, notifyPayments: true,
    notifyCoverage: false, dailySummary: true, sessionTimeout: '30', passwordLength: '10',
    twoStep: false, failedSignInAlert: true, language: 'en-PH', timeZone: 'Asia/Manila',
    dateFormat: 'dmy', currency: 'PHP'
  };
  const preferenceIds = Object.keys(defaults);
  let savedSnapshot = '';
  let toastTimer;

  function readSettings() {
    return Object.fromEntries(preferenceIds.map(id => [id, el(id).type === 'checkbox' ? el(id).checked : el(id).value]));
  }
  function settingsSnapshot() { return JSON.stringify(readSettings()); }
  function setSaveState(isDirty) {
    const state = el('saveState');
    state.textContent = isDirty ? 'Unsaved changes' : 'All changes saved';
    state.classList.toggle('unsaved', isDirty);
  }
  function toast(message) {
    const box = el('toast'); box.textContent = message; box.classList.add('show');
    window.clearTimeout(toastTimer); toastTimer = window.setTimeout(() => box.classList.remove('show'), 2500);
  }
  function applyPreview() {
    const badge = document.querySelector('.demo-pill');
    if (badge) badge.hidden = !el('demoBadge').checked;
    let banner = el('maintenancePreview');
    if (el('maintenanceNotice').checked) {
      if (!banner) {
        banner = document.createElement('div'); banner.id = 'maintenancePreview'; banner.className = 'maintenance-preview';
        banner.innerHTML = '<span>⚙</span><p><strong>Maintenance notice preview</strong> · This is sample workspace content.</p>';
        el('settingsDemoBanner').after(banner);
      }
    } else if (banner) banner.remove();
  }
  function save() {
    if (!el('settingsForm').reportValidity()) return;
    savedSnapshot = settingsSnapshot(); setSaveState(false); applyPreview();
    el('lastSavedNote').textContent = 'Saved just now · Current page session only';
    toast('Settings saved for this demo session.');
  }
  function closeResetModal() { el('resetModal').hidden = true; }
  function resetToDefaults() {
    preferenceIds.forEach(id => {
      const input = el(id);
      if (input.type === 'checkbox') input.checked = defaults[id];
      else input.value = defaults[id];
    });
    closeResetModal(); save();
    toast('Default demo settings restored.');
  }

  savedSnapshot = settingsSnapshot();
  el('settingsForm').addEventListener('input', () => setSaveState(settingsSnapshot() !== savedSnapshot));
  el('settingsForm').addEventListener('change', () => setSaveState(settingsSnapshot() !== savedSnapshot));
  el('saveSettings').addEventListener('click', save);
  el('saveSettingsBottom').addEventListener('click', save);
  el('resetSettings').addEventListener('click', () => { el('resetModal').hidden = false; });
  el('resetSettingsBottom').addEventListener('click', () => { el('resetModal').hidden = false; });
  el('confirmReset').addEventListener('click', resetToDefaults);
  document.querySelectorAll('[data-close-modal]').forEach(button => button.addEventListener('click', closeResetModal));
  el('resetModal').addEventListener('click', event => { if (event.target === el('resetModal')) closeResetModal(); });
  document.addEventListener('keydown', event => { if (event.key === 'Escape') closeResetModal(); });

  const sectionLinks = [...document.querySelectorAll('.settings-nav-link')];
  sectionLinks.forEach(link => link.addEventListener('click', () => {
    sectionLinks.forEach(item => item.classList.toggle('active', item === link));
  }));
  if ('IntersectionObserver' in window) {
    const observer = new IntersectionObserver(entries => {
      const visible = entries.filter(entry => entry.isIntersecting).sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0];
      if (!visible) return;
      const active = sectionLinks.find(link => link.dataset.section === visible.target.id);
      if (active) sectionLinks.forEach(link => link.classList.toggle('active', link === active));
    }, { rootMargin: '-20% 0px -65% 0px', threshold: [0, .2, .6] });
    ['general', 'notifications', 'security', 'localization'].forEach(id => observer.observe(el(id)));
  }
  document.querySelectorAll('.future-link, [data-toast]').forEach(link => link.addEventListener('click', event => {
    event.preventDefault(); toast(link.dataset.toast || `${link.dataset.page} is planned as a future admin page.`);
  }));
  el('helpButton')?.addEventListener('click', () => toast('Settings are sample preferences for this frontend demo.'));
  el('notificationButton')?.addEventListener('click', () => toast('No new settings notices in this demo.'));
  const sidebar = el('sidebar'); const scrim = el('sidebarScrim');
  el('mobileMenu')?.addEventListener('click', () => { sidebar.classList.add('open'); scrim.classList.add('open'); });
  scrim?.addEventListener('click', () => { sidebar.classList.remove('open'); scrim.classList.remove('open'); });
})();
