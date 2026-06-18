import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useDropzone } from 'react-dropzone';
import * as THREE from 'three';
import { STLLoader } from 'three/examples/jsm/loaders/STLLoader.js';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { UploadCloud, X } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface Props {
  onFileSelect: (file: File | null) => void;
}

export const FileUploader: React.FC<Props> = ({ onFileSelect }) => {
  const { t } = useTranslation();
  const [file, setFile] = useState<File | null>(null);
  const mountRef = useRef<HTMLDivElement>(null);
  
  const onDrop = useCallback((acceptedFiles: File[]) => {
    if (acceptedFiles.length > 0) {
      const selected = acceptedFiles[0];
      setFile(selected);
      onFileSelect(selected);
    }
  }, [onFileSelect]);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'model/stl': ['.stl'],
      'model/3mf': ['.3mf'],
      'model/obj': ['.obj']
    },
    maxSize: 50 * 1024 * 1024, // 50MB
    maxFiles: 1
  });

  const handleRemove = (e: React.MouseEvent) => {
    e.stopPropagation();
    setFile(null);
    onFileSelect(null);
  };

  useEffect(() => {
    if (!file || !mountRef.current || !file.name.toLowerCase().endsWith('.stl')) return;

    let width = mountRef.current.clientWidth;
    let height = 300;

    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0xf9fafb);

    const camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 1000);
    const renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setSize(width, height);
    mountRef.current.innerHTML = '';
    mountRef.current.appendChild(renderer.domElement);

    const controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;

    const light = new THREE.HemisphereLight(0xffffff, 0x444444, 1.0);
    light.position.set(0, 1, 0);
    scene.add(light);
    const dirLight = new THREE.DirectionalLight(0xffffff, 0.5);
    dirLight.position.set(0, 1, 0);
    scene.add(dirLight);

    const loader = new STLLoader();
    const reader = new FileReader();

    reader.onload = (e) => {
      if (!e.target?.result) return;
      const geometry = loader.parse(e.target.result as ArrayBuffer);
      const material = new THREE.MeshStandardMaterial({ 
        color: 0x0ea5e9, 
        roughness: 0.4, 
        metalness: 0.1 
      });
      const mesh = new THREE.Mesh(geometry, material);
      
      geometry.computeBoundingBox();
      const center = geometry.boundingBox?.getCenter(new THREE.Vector3());
      if (center) {
        mesh.position.sub(center);
      }
      
      const size = geometry.boundingBox?.getSize(new THREE.Vector3());
      const maxDim = Math.max(size?.x || 1, size?.y || 1, size?.z || 1);
      
      camera.position.z = maxDim * 2;
      scene.add(mesh);
    };

    reader.readAsArrayBuffer(file);

    const animate = () => {
      requestAnimationFrame(animate);
      controls.update();
      renderer.render(scene, camera);
    };
    animate();

    const handleResize = () => {
      if (!mountRef.current) return;
      width = mountRef.current.clientWidth;
      renderer.setSize(width, height);
      camera.aspect = width / height;
      camera.updateProjectionMatrix();
    };

    window.addEventListener('resize', handleResize);

    return () => {
      window.removeEventListener('resize', handleResize);
      renderer.dispose();
    };
  }, [file]);

  return (
    <div className="w-full">
      {!file ? (
        <div 
          {...getRootProps()} 
          className={`border-2 border-dashed rounded-xl p-10 text-center cursor-pointer transition ${isDragActive ? 'border-brand bg-brand/5' : 'border-gray-300 hover:border-brand hover:bg-gray-50'}`}
        >
          <input {...getInputProps()} />
          <UploadCloud className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <p className="text-lg font-medium text-gray-700">{t('quote.uploadStep')}</p>
          <p className="text-sm text-gray-500 mt-2">{t('quote.uploadDesc')}</p>
        </div>
      ) : (
        <div className="border rounded-xl overflow-hidden bg-white shadow-sm">
          <div className="flex justify-between items-center p-4 border-b bg-gray-50">
            <span className="font-medium truncate mr-4">{file.name}</span>
            <button onClick={handleRemove} className="text-gray-500 hover:text-red-500 bg-white p-1 rounded-full shadow-sm">
              <X className="w-5 h-5" />
            </button>
          </div>
          {file.name.toLowerCase().endsWith('.stl') ? (
             <div ref={mountRef} className="w-full h-[300px]" />
          ) : (
             <div className="w-full h-[300px] flex items-center justify-center bg-gray-100 text-gray-500">
               3D Preview not available for this format.
             </div>
          )}
        </div>
      )}
    </div>
  );
};
