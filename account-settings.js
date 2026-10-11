const $=s=>document.querySelector(s), $$=s=>[...document.querySelectorAll(s)];
const toast=(m)=>{const t=$('#toast');if(!t)return;t.textContent=m;t.classList.add('show');clearTimeout(window.__tt);window.__tt=setTimeout(()=>t.classList.remove('show'),2400)};
const state=$('#saveState');let dirty=false;
function markDirty(){dirty=true;if(state){state.textContent='Unsaved changes';state.classList.add('unsaved')}}
$$('input,textarea,select').forEach(el=>el.addEventListener('input',markDirty));
$$('.settings-nav button').forEach(btn=>btn.addEventListener('click',()=>{const key=btn.dataset.section;$$('.settings-nav button').forEach(x=>x.classList.toggle('active',x===btn));$$('.settings-section').forEach(x=>x.classList.toggle('active',x.dataset.panel===key));history.replaceState(null,'',`#${key}`)}));
const initial=location.hash.replace('#','');if(initial){const b=document.querySelector(`.settings-nav button[data-section="${initial}"]`);if(b)b.click()}
const bio=$('#bio'),bioCount=$('#bioCount');function updateBio(){if(bio){if(bio.value.length>240)bio.value=bio.value.slice(0,240);bioCount.textContent=bio.value.length}}if(bio){updateBio();bio.addEventListener('input',updateBio)}
$$('[data-toggle]').forEach(b=>b.addEventListener('click',()=>{const input=$('#'+b.dataset.toggle);input.type=input.type==='password'?'text':'password';b.textContent=input.type==='password'?'Show':'Hide'}));
$('#photoBtn')?.addEventListener('click',()=>$('#photoInput').click());$('#photoInput')?.addEventListener('change',e=>{const f=e.target.files[0];if(f){toast('Profile photo preview updated');markDirty()}});
$('#addSkill')?.addEventListener('click',()=>{const name=prompt('Add a skill or service');if(!name||!name.trim())return;const btn=document.createElement('button');btn.type='button';btn.className='skill-tag';btn.innerHTML=`${name.trim()} <b>×</b>`;btn.addEventListener('click',()=>{btn.remove();markDirty()});$('#skillEditor').insertBefore(btn,$('#addSkill'));markDirty()});
$$('.skill-tag').forEach(btn=>btn.addEventListener('click',()=>{btn.remove();markDirty()}));
$('#saveBtn')?.addEventListener('click',()=>{const first=$('#firstName').value.trim(),last=$('#lastName').value.trim(),email=$('#email').value.trim();if(!first||!last||!email){toast('Please complete the required profile fields');return}const np=$('#newPassword').value,cp=$('#confirmPassword').value;if(np&&np!==cp){toast('New passwords do not match');return}localStorage.setItem('tasktapAccountSettings',JSON.stringify({first,last,email,bio:bio?.value||'',savedAt:Date.now()}));dirty=false;if(state){state.textContent='All changes saved';state.classList.remove('unsaved')}toast('Your account settings were saved')});
$('#signOutOthers')?.addEventListener('click',()=>toast('Other demo sessions were signed out'));
$('#deactivateBtn')?.addEventListener('click',()=>{if(confirm('Deactivate this demo account?'))toast('Demo account deactivation request submitted')});
window.addEventListener('beforeunload',e=>{if(dirty){e.preventDefault();e.returnValue=''}});
