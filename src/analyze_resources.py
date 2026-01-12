#!/usr/bin/env python3
# analyze_resources.py

import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import sys
import os
import re

def parse_memory(memory_str):
    """Преобразует строку памяти '33.38MiB / 2GiB' в МБ"""
    try:
        if pd.isna(memory_str) or memory_str == '':
            return 0
        
        # Парсим первое значение (текущее использование)
        match = re.search(r'([\d.]+)\s*([KMG]i?B)', str(memory_str))
        if match:
            value = float(match.group(1))
            unit = match.group(2)
            
            # Конвертируем в МБ
            units = {
                'B': 1/1024/1024,
                'KiB': 1/1024,
                'KB': 1/1024,
                'MiB': 1,
                'MB': 1,
                'GiB': 1024,
                'GB': 1024
            }
            
            return value * units.get(unit, 1)
        return 0
    except Exception as e:
        print(f"Ошибка при парсинге памяти '{memory_str}': {e}")
        return 0

def parse_percentage(percent_str):
    """Преобразует строку процентов '2.81%' в число"""
    try:
        if pd.isna(percent_str) or percent_str == '':
            return 0
        return float(str(percent_str).rstrip('%'))
    except:
        return 0

def parse_io(io_str):
    """Преобразует строку I/O '1.44kB / 599B' в МБ (берет максимум)"""
    try:
        if pd.isna(io_str) or io_str == '':
            return 0
        
        # Парсим оба значения (read / write)
        parts = str(io_str).split('/')
        max_io = 0
        
        for part in parts:
            match = re.search(r'([\d.]+)\s*([KMG]i?B)', part.strip())
            if match:
                value = float(match.group(1))
                unit = match.group(2)
                
                units = {
                    'B': 1/1024/1024,
                    'KiB': 1/1024,
                    'KB': 1/1024,
                    'MiB': 1,
                    'MB': 1,
                    'GiB': 1024,
                    'GB': 1024
                }
                
                value_mb = value * units.get(unit, 1)
                max_io = max(max_io, value_mb)
        
        return max_io
    except:
        return 0

def analyze_resources(results_dir):
    """Анализирует файл resource_usage.csv и выводит статистику"""
    
    csv_file = f'{results_dir}/resource_usage.csv'
    
    if not os.path.exists(csv_file):
        print(f"Файл {csv_file} не найден")
        return
    
    print("Анализ использования ресурсов...")
    print(f"Файл: {csv_file}\n")
    
    # Загружаем данные
    try:
        df = pd.read_csv(csv_file)
    except Exception as e:
        print(f"Ошибка при чтении CSV: {e}")
        return
    
    if df.empty:
        print("Файл пуст")
        return
    
    print(f"Всего записей: {len(df)}")
    print(f"Контейнеры: {df['container'].unique().tolist()}\n")
    
    # Парсим данные
    df['cpu'] = df['cpu_percent'].apply(parse_percentage)
    df['mem_mb'] = df['mem_usage'].apply(parse_memory)
    df['mem_percent'] = df['mem_percent'].apply(parse_percentage)
    df['io_mb'] = df['net_io'].apply(parse_io)
    df['disk_io_mb'] = df['block_io'].apply(parse_io)
    
    # Группируем по контейнерам
    containers = df['container'].unique()
    
    print("=" * 90)
    print("СТАТИСТИКА ИСПОЛЬЗОВАНИЯ РЕСУРСОВ ПО КОМПОНЕНТАМ")
    print("=" * 90)
    
    stats_data = []
    
    for container in containers:
        container_data = df[df['container'] == container]
        
        # Вычисляем статистику
        cpu_min = container_data['cpu'].min()
        cpu_max = container_data['cpu'].max()
        cpu_avg = container_data['cpu'].mean()
        
        mem_min = container_data['mem_mb'].min()
        mem_max = container_data['mem_mb'].max()
        mem_avg = container_data['mem_mb'].mean()
        mem_percent_avg = container_data['mem_percent'].mean()
        
        io_max = container_data['io_mb'].max()
        io_avg = container_data['io_mb'].mean()
        
        disk_io_max = container_data['disk_io_mb'].max()
        disk_io_avg = container_data['disk_io_mb'].mean()
        
        pids_max = container_data['pids'].max()
        pids_avg = container_data['pids'].mean()
        
        # Выводим информацию
        print(f"\nКомпонент: {container}")
        print("-" * 90)
        print(f"  CPU:          min={cpu_min:7.2f}% | avg={cpu_avg:7.2f}% | max={cpu_max:7.2f}%")
        print(f"  RAM (МБ):     min={mem_min:7.2f} | avg={mem_avg:7.2f} | max={mem_max:7.2f}")
        print(f"  RAM (%):      avg={mem_percent_avg:6.2f}%")
        print(f"  Network I/O:  avg={io_avg:7.3f} МБ | max={io_max:7.3f} МБ")
        print(f"  Disk I/O:     avg={disk_io_avg:7.3f} МБ | max={disk_io_max:7.3f} МБ")
        print(f"  Процессы:     avg={pids_avg:6.1f} | max={pids_max:6.0f}")
        
        stats_data.append({
            'container': container,
            'cpu_min': cpu_min,
            'cpu_avg': cpu_avg,
            'cpu_max': cpu_max,
            'mem_min': mem_min,
            'mem_avg': mem_avg,
            'mem_max': mem_max,
            'mem_percent_avg': mem_percent_avg,
            'io_avg': io_avg,
            'io_max': io_max,
            'disk_io_avg': disk_io_avg,
            'disk_io_max': disk_io_max,
            'pids_avg': pids_avg,
            'pids_max': pids_max
        })
    
    # Создаем графики
    create_graphs(results_dir, df, containers)
    
    print("\n" + "=" * 90)
    
    return stats_data

def create_graphs(results_dir, df, containers):
    """Создает графики использования ресурсов"""
    
    print("\nСоздание графиков...")
    
    # Конвертируем timestamp в числовой формат для построения графика
    df['time_num'] = pd.to_numeric(df['timestamp'].str.replace('N', ''), errors='coerce')
    df['time_sec'] = (df['time_num'] - df['time_num'].min()) / 1000  # переводим в секунды
    
    # Создаем графики для каждого контейнера
    fig, axes = plt.subplots(len(containers), 3, figsize=(18, 5*len(containers)))
    
    if len(containers) == 1:
        axes = axes.reshape(1, -1)
    
    colors = {'src-app-1': 'blue', 'src-db-1': 'red'}
    
    for idx, container in enumerate(containers):
        container_data = df[df['container'] == container].sort_values('time_sec')
        
        if len(container_data) == 0:
            continue
        
        color = colors.get(container, 'green')
        
        # График 1: CPU
        ax = axes[idx, 0]
        ax.plot(container_data['time_sec'], container_data['cpu'], 
                linewidth=2, color=color, marker='o', markersize=3)
        ax.fill_between(container_data['time_sec'], container_data['cpu'], alpha=0.3, color=color)
        ax.set_title(f'{container} - CPU Usage', fontsize=12, fontweight='bold')
        ax.set_ylabel('CPU (%)', fontsize=10)
        ax.set_xlabel('Время (сек)', fontsize=10)
        ax.grid(True, alpha=0.3)
        
        # Добавляем min/max/avg
        cpu_min = container_data['cpu'].min()
        cpu_max = container_data['cpu'].max()
        cpu_avg = container_data['cpu'].mean()
        ax.text(0.02, 0.98, f'Min: {cpu_min:.1f}%\nAvg: {cpu_avg:.1f}%\nMax: {cpu_max:.1f}%',
                transform=ax.transAxes, verticalalignment='top',
                bbox=dict(boxstyle='round', facecolor='wheat', alpha=0.5), fontsize=9)
        
        # График 2: RAM
        ax = axes[idx, 1]
        ax.plot(container_data['time_sec'], container_data['mem_mb'], 
                linewidth=2, color=color, marker='s', markersize=3, label='RAM (МБ)')
        ax.fill_between(container_data['time_sec'], container_data['mem_mb'], alpha=0.3, color=color)
        
        ax2 = ax.twinx()
        ax2.plot(container_data['time_sec'], container_data['mem_percent'], 
                linewidth=2, color='orange', marker='^', markersize=3, linestyle='--', label='RAM (%)')
        
        ax.set_title(f'{container} - Memory Usage', fontsize=12, fontweight='bold')
        ax.set_ylabel('RAM (МБ)', fontsize=10, color=color)
        ax2.set_ylabel('RAM (%)', fontsize=10, color='orange')
        ax.set_xlabel('Время (сек)', fontsize=10)
        ax.grid(True, alpha=0.3)
        
        mem_min = container_data['mem_mb'].min()
        mem_max = container_data['mem_mb'].max()
        mem_avg = container_data['mem_mb'].mean()
        ax.text(0.02, 0.98, f'Min: {mem_min:.1f} МБ\nAvg: {mem_avg:.1f} МБ\nMax: {mem_max:.1f} МБ',
                transform=ax.transAxes, verticalalignment='top',
                bbox=dict(boxstyle='round', facecolor='lightblue', alpha=0.5), fontsize=9)
        
        # График 3: I/O
        ax = axes[idx, 2]
        ax.plot(container_data['time_sec'], container_data['io_mb'], 
                linewidth=2, color='green', marker='o', markersize=3, label='Network I/O')
        ax.plot(container_data['time_sec'], container_data['disk_io_mb'], 
                linewidth=2, color='purple', marker='s', markersize=3, label='Disk I/O')
        ax.fill_between(container_data['time_sec'], container_data['io_mb'], alpha=0.2, color='green')
        ax.fill_between(container_data['time_sec'], container_data['disk_io_mb'], alpha=0.2, color='purple')
        
        ax.set_title(f'{container} - I/O Operations', fontsize=12, fontweight='bold')
        ax.set_ylabel('I/O (МБ)', fontsize=10)
        ax.set_xlabel('Время (сек)', fontsize=10)
        ax.legend(loc='upper left', fontsize=9)
        ax.grid(True, alpha=0.3)
        
        net_max = container_data['io_mb'].max()
        disk_max = container_data['disk_io_mb'].max()
        ax.text(0.02, 0.98, f'Network Max: {net_max:.3f} МБ\nDisk Max: {disk_max:.3f} МБ',
                transform=ax.transAxes, verticalalignment='top',
                bbox=dict(boxstyle='round', facecolor='lightyellow', alpha=0.5), fontsize=9)
    
    plt.tight_layout()
    graph_path = f'{results_dir}/resource_analysis_graphs.png'
    plt.savefig(graph_path, dpi=300, bbox_inches='tight')
    plt.close()
    
    print(f"Графики сохранены: {graph_path}")

def create_summary_table(results_dir, stats_data):
    """Создает сводную таблицу в виде графика"""
    
    if not stats_data:
        return
    
    fig, ax = plt.subplots(figsize=(14, 6))
    ax.axis('tight')
    ax.axis('off')
    
    # Подготавливаем данные для таблицы
    table_data = []
    headers = ['Компонент', 'CPU (min/avg/max)', 'RAM МБ (min/avg/max)', 
               'RAM % (avg)', 'Network I/O (avg/max)', 'Disk I/O (avg/max)', 'Процессы (avg/max)']
    
    for stats in stats_data:
        row = [
            stats['container'],
            f"{stats['cpu_min']:.1f}% / {stats['cpu_avg']:.1f}% / {stats['cpu_max']:.1f}%",
            f"{stats['mem_min']:.1f} / {stats['mem_avg']:.1f} / {stats['mem_max']:.1f}",
            f"{stats['mem_percent_avg']:.2f}%",
            f"{stats['io_avg']:.3f} / {stats['io_max']:.3f} МБ",
            f"{stats['disk_io_avg']:.3f} / {stats['disk_io_max']:.3f} МБ",
            f"{stats['pids_avg']:.1f} / {stats['pids_max']:.0f}"
        ]
        table_data.append(row)
    
    table = ax.table(cellText=table_data, colLabels=headers, cellLoc='center', 
                     loc='center', bbox=[0, 0, 1, 1])
    
    table.auto_set_font_size(False)
    table.set_fontsize(9)
    table.scale(1, 2)
    
    # Стилизация заголовка
    for i in range(len(headers)):
        table[(0, i)].set_facecolor('#4CAF50')
        table[(0, i)].set_text_props(weight='bold', color='white')
    
    # Стилизация строк
    for i in range(1, len(table_data) + 1):
        for j in range(len(headers)):
            if i % 2 == 0:
                table[(i, j)].set_facecolor('#f0f0f0')
            else:
                table[(i, j)].set_facecolor('#ffffff')
    
    plt.title('Сводная таблица использования ресурсов', fontsize=14, fontweight='bold', pad=20)
    plt.tight_layout()
    
    table_path = f'{results_dir}/resource_summary_table.png'
    plt.savefig(table_path, dpi=300, bbox_inches='tight')
    plt.close()
    
    print(f"Сводная таблица сохранена: {table_path}")

if __name__ == '__main__':
    if len(sys.argv) > 1:
        results_dir = sys.argv[1]
    else:
        # Используем директорию с первым найденным результатом
        results_dir = '.'
    
    stats_data = analyze_resources(results_dir)
    if stats_data:
        create_summary_table(results_dir, stats_data)
        print("\nАнализ завершен!")
